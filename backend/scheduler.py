from datetime import datetime, timedelta
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.cron import CronTrigger
from sqlalchemy.orm import Session
from contextlib import contextmanager

from database import SessionLocal
from models import Installment, Client
from email_service import email_service

class PaymentReminderScheduler:
    def __init__(self):
        self.scheduler = AsyncIOScheduler()
        self.setup_jobs()
    
    def setup_jobs(self):
        # Send reminders every day at 9:00 AM
        self.scheduler.add_job(
            self.send_payment_reminders,
            CronTrigger(hour=9, minute=0),
            id='daily_reminders',
            name='Send daily payment reminders',
            replace_existing=True
        )
        
        # Send overdue reminders every day at 10:00 AM
        self.scheduler.add_job(
            self.send_overdue_reminders,
            CronTrigger(hour=10, minute=0),
            id='overdue_reminders',
            name='Send overdue payment reminders',
            replace_existing=True
        )
    
    @contextmanager
    def get_db_session(self):
        db = SessionLocal()
        try:
            yield db
        finally:
            db.close()
    
    async def send_payment_reminders(self):
        print(f"Running payment reminder job at {datetime.now()}")
        
        with self.get_db_session() as db:
            # Get installments due in the next 3 days that haven't been paid
            three_days_from_now = datetime.now() + timedelta(days=3)
            
            upcoming_installments = db.query(Installment).join(Client).filter(
                Installment.is_paid == False,
                Installment.due_date <= three_days_from_now,
                Installment.due_date >= datetime.now(),
                Installment.reminder_sent == False
            ).all()
            
            for installment in upcoming_installments:
                try:
                    success = email_service.send_reminder_email(installment.client, installment)
                    
                    if success:
                        installment.reminder_sent = True
                        installment.last_reminder_sent = datetime.now()
                        db.commit()
                        
                        email_service.log_email(
                            db, 
                            installment.client_id, 
                            installment.id, 
                            'reminder', 
                            'sent'
                        )
                        print(f"Reminder sent to {installment.client.email} for installment {installment.id}")
                    else:
                        email_service.log_email(
                            db, 
                            installment.client_id, 
                            installment.id, 
                            'reminder', 
                            'failed',
                            'SendGrid API error'
                        )
                        
                except Exception as e:
                    print(f"Error processing installment {installment.id}: {str(e)}")
                    email_service.log_email(
                        db, 
                        installment.client_id, 
                        installment.id, 
                        'reminder', 
                        'failed',
                        str(e)
                    )
    
    async def send_overdue_reminders(self):
        print(f"Running overdue reminder job at {datetime.now()}")
        
        with self.get_db_session() as db:
            # Get installments that are overdue
            now = datetime.now()
            
            overdue_installments = db.query(Installment).join(Client).filter(
                Installment.is_paid == False,
                Installment.due_date < now
            ).all()
            
            for installment in overdue_installments:
                try:
                    # Send overdue email if it's been more than 7 days since last reminder
                    days_since_last_reminder = None
                    if installment.last_reminder_sent:
                        days_since_last_reminder = (now - installment.last_reminder_sent).days
                    
                    if not days_since_last_reminder or days_since_last_reminder >= 7:
                        success = email_service.send_overdue_email(installment.client, installment)
                        
                        if success:
                            installment.last_reminder_sent = datetime.now()
                            db.commit()
                            
                            email_service.log_email(
                                db, 
                                installment.client_id, 
                                installment.id, 
                                'overdue', 
                                'sent'
                            )
                            print(f"Overdue reminder sent to {installment.client.email} for installment {installment.id}")
                        else:
                            email_service.log_email(
                                db, 
                                installment.client_id, 
                                installment.id, 
                                'overdue', 
                                'failed',
                                'SendGrid API error'
                            )
                            
                except Exception as e:
                    print(f"Error processing overdue installment {installment.id}: {str(e)}")
                    email_service.log_email(
                        db, 
                        installment.client_id, 
                        installment.id, 
                        'overdue', 
                        'failed',
                        str(e)
                    )
    
    def start(self):
        self.scheduler.start()
        print("Payment reminder scheduler started")
    
    def stop(self):
        self.scheduler.shutdown()
        print("Payment reminder scheduler stopped")

# Global scheduler instance
payment_scheduler = PaymentReminderScheduler()
