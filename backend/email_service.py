import os
from datetime import datetime
from typing import List
from sendgrid import SendGridAPIClient
from sendgrid.helpers.mail import Mail
from sqlalchemy.orm import Session
from dotenv import load_dotenv

from models import Client, Installment, EmailLog

load_dotenv()

SENDGRID_API_KEY = os.getenv("SENDGRID_API_KEY")
FROM_EMAIL = os.getenv("FROM_EMAIL")
PAYMENT_LINK = os.getenv("PAYMENT_LINK")

class EmailService:
    def __init__(self):
        self.sg = SendGridAPIClient(SENDGRID_API_KEY) if SENDGRID_API_KEY else None
    
    def send_reminder_email(self, client: Client, installment: Installment) -> bool:
        try:
            subject = f"Payment Reminder - {installment.description or 'Installment Due'}"
            
            html_content = f"""
            <html>
            <body>
                <h2>Payment Reminder</h2>
                <p>Dear {client.name},</p>
                <p>This is a friendly reminder that you have a payment due:</p>
                <ul>
                    <li><strong>Amount:</strong> ${installment.amount:.2f}</li>
                    <li><strong>Due Date:</strong> {installment.due_date.strftime('%B %d, %Y')}</li>
                    <li><strong>Description:</strong> {installment.description or 'Installment Payment'}</li>
                </ul>
                <p>Please click the link below to make your payment:</p>
                <p><a href="{PAYMENT_LINK}?client_id={client.id}&installment_id={installment.id}" style="background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Pay Now</a></p>
                <p>If you have already made this payment, please disregard this email.</p>
                <p>Thank you for your business!</p>
                <p>Best regards,<br>Your Payment Team</p>
            </body>
            </html>
            """
            
            message = Mail(
                from_email=FROM_EMAIL,
                to_emails=client.email,
                subject=subject,
                html_content=html_content
            )
            
            response = self.sg.send(message)
            return response.status_code == 202
            
        except Exception as e:
            print(f"Error sending email to {client.email}: {str(e)}")
            return False
    
    def send_overdue_email(self, client: Client, installment: Installment) -> bool:
        try:
            subject = f"URGENT: Overdue Payment - {installment.description or 'Installment'}"
            
            html_content = f"""
            <html>
            <body>
                <h2 style="color: red;">URGENT: Overdue Payment</h2>
                <p>Dear {client.name},</p>
                <p>This is an urgent notice that the following payment is overdue:</p>
                <ul>
                    <li><strong>Amount:</strong> ${installment.amount:.2f}</li>
                    <li><strong>Due Date:</strong> {installment.due_date.strftime('%B %d, %Y')}</li>
                    <li><strong>Days Overdue:</strong> {(datetime.now() - installment.due_date).days} days</li>
                    <li><strong>Description:</strong> {installment.description or 'Installment Payment'}</li>
                </ul>
                <p>Please make your payment immediately to avoid additional fees:</p>
                <p><a href="{PAYMENT_LINK}?client_id={client.id}&installment_id={installment.id}" style="background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Pay Now</a></p>
                <p>If you have already made this payment, please contact us immediately.</p>
                <p>Best regards,<br>Your Payment Team</p>
            </body>
            </html>
            """
            
            message = Mail(
                from_email=FROM_EMAIL,
                to_emails=client.email,
                subject=subject,
                html_content=html_content
            )
            
            response = self.sg.send(message)
            return response.status_code == 202
            
        except Exception as e:
            print(f"Error sending overdue email to {client.email}: {str(e)}")
            return False
    
    def log_email(self, db: Session, client_id: int, installment_id: int, email_type: str, status: str, error_message: str = None):
        email_log = EmailLog(
            client_id=client_id,
            installment_id=installment_id,
            email_type=email_type,
            status=status,
            error_message=error_message
        )
        db.add(email_log)
        db.commit()

email_service = EmailService()
