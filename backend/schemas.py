from pydantic import BaseModel, EmailStr
from datetime import datetime
from typing import List, Optional

# User schemas
class UserBase(BaseModel):
    username: str
    email: EmailStr

class UserCreate(UserBase):
    password: str

class User(UserBase):
    id: int
    is_active: bool
    created_at: datetime
    
    class Config:
        from_attributes = True

# Client schemas
class ClientBase(BaseModel):
    name: str
    email: EmailStr
    phone: Optional[str] = None
    address: Optional[str] = None

class ClientCreate(ClientBase):
    pass

class Client(ClientBase):
    id: int
    created_at: datetime
    updated_at: Optional[datetime] = None
    
    class Config:
        from_attributes = True

# Installment schemas
class InstallmentBase(BaseModel):
    amount: float
    due_date: datetime
    description: Optional[str] = None

class InstallmentCreate(InstallmentBase):
    client_id: int

class Installment(InstallmentBase):
    id: int
    client_id: int
    is_paid: bool
    paid_at: Optional[datetime] = None
    reminder_sent: bool
    last_reminder_sent: Optional[datetime] = None
    created_at: datetime
    client: Client
    
    class Config:
        from_attributes = True

# Client with installments
class ClientWithInstallments(Client):
    installments: List[Installment] = []

# Email Log schemas
class EmailLogBase(BaseModel):
    client_id: int
    installment_id: int
    email_type: str
    status: str
    error_message: Optional[str] = None

class EmailLog(EmailLogBase):
    id: int
    sent_at: datetime
    
    class Config:
        from_attributes = True

# Authentication schemas
class Token(BaseModel):
    access_token: str
    token_type: str

class TokenData(BaseModel):
    username: Optional[str] = None
