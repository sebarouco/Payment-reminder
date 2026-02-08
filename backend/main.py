from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.orm import Session
from datetime import timedelta
import uvicorn

from database import get_db, engine
from models import Base
from schemas import UserCreate, User, Token, ClientCreate, Client, ClientWithInstallments, InstallmentCreate, Installment
from auth import authenticate_user, create_access_token, get_current_active_user, get_password_hash, ACCESS_TOKEN_EXPIRE_MINUTES
from scheduler import payment_scheduler

# Create database tables
Base.metadata.create_all(bind=engine)

app = FastAPI(title="Payment Reminder System", version="1.0.0")

@app.on_event("startup")
async def startup_event():
    payment_scheduler.start()

@app.on_event("shutdown")
async def shutdown_event():
    payment_scheduler.stop()

# Authentication routes
@app.post("/token", response_model=Token)
async def login_for_access_token(form_data: OAuth2PasswordRequestForm = Depends(), db: Session = Depends(get_db)):
    user = authenticate_user(db, form_data.username, form_data.password)
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect username or password",
            headers={"WWW-Authenticate": "Bearer"},
        )
    access_token_expires = timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": user.username}, expires_delta=access_token_expires
    )
    return {"access_token": access_token, "token_type": "bearer"}

@app.post("/users/", response_model=User)
async def create_user(user: UserCreate, db: Session = Depends(get_db)):
    from models import User
    
    # Check if user already exists
    db_user = db.query(User).filter(User.email == user.email).first()
    if db_user:
        raise HTTPException(status_code=400, detail="Email already registered")
    
    db_user = db.query(User).filter(User.username == user.username).first()
    if db_user:
        raise HTTPException(status_code=400, detail="Username already taken")
    
    # Create new user
    hashed_password = get_password_hash(user.password)
    db_user = User(
        username=user.username,
        email=user.email,
        hashed_password=hashed_password
    )
    db.add(db_user)
    db.commit()
    db.refresh(db_user)
    return db_user

@app.get("/users/me", response_model=User)
async def read_users_me(current_user: User = Depends(get_current_active_user)):
    return current_user

# Client routes
@app.post("/clients/", response_model=Client)
async def create_client(client: ClientCreate, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client
    
    # Check if client email already exists
    db_client = db.query(Client).filter(Client.email == client.email).first()
    if db_client:
        raise HTTPException(status_code=400, detail="Client email already exists")
    
    db_client = Client(
        name=client.name,
        email=client.email,
        phone=client.phone,
        address=client.address,
        created_by=current_user.id
    )
    db.add(db_client)
    db.commit()
    db.refresh(db_client)
    return db_client

@app.get("/clients/", response_model=list[Client])
async def get_clients(skip: int = 0, limit: int = 100, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client
    clients = db.query(Client).offset(skip).limit(limit).all()
    return clients

@app.get("/clients/{client_id}", response_model=ClientWithInstallments)
async def get_client(client_id: int, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client
    client = db.query(Client).filter(Client.id == client_id).first()
    if client is None:
        raise HTTPException(status_code=404, detail="Client not found")
    return client

@app.put("/clients/{client_id}", response_model=Client)
async def update_client(client_id: int, client_update: ClientCreate, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client
    
    client = db.query(Client).filter(Client.id == client_id).first()
    if client is None:
        raise HTTPException(status_code=404, detail="Client not found")
    
    # Check if email is being changed and if new email already exists
    if client.email != client_update.email:
        existing_client = db.query(Client).filter(Client.email == client_update.email).first()
        if existing_client:
            raise HTTPException(status_code=400, detail="Email already exists")
    
    client.name = client_update.name
    client.email = client_update.email
    client.phone = client_update.phone
    client.address = client_update.address
    
    db.commit()
    db.refresh(client)
    return client

@app.delete("/clients/{client_id}")
async def delete_client(client_id: int, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client
    
    client = db.query(Client).filter(Client.id == client_id).first()
    if client is None:
        raise HTTPException(status_code=404, detail="Client not found")
    
    db.delete(client)
    db.commit()
    return {"message": "Client deleted successfully"}

# Installment routes
@app.post("/installments/", response_model=Installment)
async def create_installment(installment: InstallmentCreate, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment, Client
    
    # Check if client exists
    client = db.query(Client).filter(Client.id == installment.client_id).first()
    if not client:
        raise HTTPException(status_code=404, detail="Client not found")
    
    db_installment = Installment(
        client_id=installment.client_id,
        amount=installment.amount,
        due_date=installment.due_date,
        description=installment.description
    )
    db.add(db_installment)
    db.commit()
    db.refresh(db_installment)
    return db_installment

@app.get("/installments/", response_model=list[Installment])
async def get_installments(skip: int = 0, limit: int = 100, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    installments = db.query(Installment).offset(skip).limit(limit).all()
    return installments

@app.get("/installments/{installment_id}", response_model=Installment)
async def get_installment(installment_id: int, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    installment = db.query(Installment).filter(Installment.id == installment_id).first()
    if installment is None:
        raise HTTPException(status_code=404, detail="Installment not found")
    return installment

@app.put("/installments/{installment_id}/mark-paid")
async def mark_installment_paid(installment_id: int, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    from datetime import datetime
    
    installment = db.query(Installment).filter(Installment.id == installment_id).first()
    if installment is None:
        raise HTTPException(status_code=404, detail="Installment not found")
    
    installment.is_paid = True
    installment.paid_at = datetime.now()
    
    db.commit()
    db.refresh(installment)
    return installment

@app.put("/installments/{installment_id}", response_model=Installment)
async def update_installment(installment_id: int, installment_update: InstallmentCreate, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment, Client
    
    installment = db.query(Installment).filter(Installment.id == installment_id).first()
    if installment is None:
        raise HTTPException(status_code=404, detail="Installment not found")
    
    # Check if client exists
    client = db.query(Client).filter(Client.id == installment_update.client_id).first()
    if not client:
        raise HTTPException(status_code=404, detail="Client not found")
    
    installment.client_id = installment_update.client_id
    installment.amount = installment_update.amount
    installment.due_date = installment_update.due_date
    installment.description = installment_update.description
    
    db.commit()
    db.refresh(installment)
    return installment

@app.delete("/installments/{installment_id}")
async def delete_installment(installment_id: int, db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    
    installment = db.query(Installment).filter(Installment.id == installment_id).first()
    if installment is None:
        raise HTTPException(status_code=404, detail="Installment not found")
    
    db.delete(installment)
    db.commit()
    return {"message": "Installment deleted successfully"}

# Dashboard routes
@app.get("/dashboard/summary")
async def get_dashboard_summary(db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Client, Installment
    from datetime import datetime
    
    total_clients = db.query(Client).count()
    total_installments = db.query(Installment).count()
    paid_installments = db.query(Installment).filter(Installment.is_paid == True).count()
    outstanding_installments = total_installments - paid_installments
    
    # Get overdue installments
    now = datetime.now()
    overdue_installments = db.query(Installment).filter(
        Installment.is_paid == False,
        Installment.due_date < now
    ).count()
    
    # Get upcoming installments (next 30 days)
    thirty_days_from_now = now + timedelta(days=30)
    upcoming_installments = db.query(Installment).filter(
        Installment.is_paid == False,
        Installment.due_date >= now,
        Installment.due_date <= thirty_days_from_now
    ).count()
    
    return {
        "total_clients": total_clients,
        "total_installments": total_installments,
        "paid_installments": paid_installments,
        "outstanding_installments": outstanding_installments,
        "overdue_installments": overdue_installments,
        "upcoming_installments": upcoming_installments
    }

@app.get("/dashboard/overdue-installments", response_model=list[Installment])
async def get_overdue_installments(db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    from datetime import datetime
    
    now = datetime.now()
    overdue_installments = db.query(Installment).filter(
        Installment.is_paid == False,
        Installment.due_date < now
    ).all()
    
    return overdue_installments

@app.get("/dashboard/upcoming-installments", response_model=list[Installment])
async def get_upcoming_installments(db: Session = Depends(get_db), current_user: User = Depends(get_current_active_user)):
    from models import Installment
    from datetime import datetime, timedelta
    
    now = datetime.now()
    thirty_days_from_now = now + timedelta(days=30)
    upcoming_installments = db.query(Installment).filter(
        Installment.is_paid == False,
        Installment.due_date >= now,
        Installment.due_date <= thirty_days_from_now
    ).order_by(Installment.due_date).all()
    
    return upcoming_installments

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
