# Payment Reminder System

A comprehensive payment reminder system built with FastAPI backend and Next.js frontend that automatically sends email reminders to clients for their installment payments.

## Features

### Backend (FastAPI)
- **REST API** for managing clients and installments
- **JWT Authentication** for secure admin access
- **Automated Email Reminders** using APScheduler and SendGrid
- **Database Management** with SQLAlchemy
- **Email Logging** to track sent reminders

### Frontend (Next.js)
- **Dashboard** with payment overview and statistics
- **Online Spreadsheet** interface for customer information
- **Client Management** (CRUD operations)
- **Installment Management** with payment tracking
- **Responsive Design** with Tailwind CSS

### Key Features
- ✅ Automated payment reminders (daily at 9 AM)
- ✅ Overdue payment alerts (daily at 10 AM)
- ✅ Spreadsheet-style data entry and editing
- ✅ Real-time payment status updates
- ✅ Email integration with SendGrid
- ✅ Secure authentication system

## Project Structure

```
payment-reminder-system/
├── backend/
│   ├── main.py              # FastAPI application
│   ├── models.py            # Database models
│   ├── schemas.py           # Pydantic schemas
│   ├── auth.py              # JWT authentication
│   ├── email_service.py     # Email sending logic
│   ├── scheduler.py         # APScheduler setup
│   ├── database.py          # Database configuration
│   ├── requirements.txt     # Python dependencies
│   └── .env.example        # Environment variables template
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── page.tsx            # Home page (redirects)
│   │   │   ├── login/page.tsx       # Login page
│   │   │   ├── dashboard/page.tsx    # Dashboard
│   │   │   ├── spreadsheet/page.tsx  # Online spreadsheet
│   │   │   ├── clients/page.tsx     # Client management
│   │   │   ├── installments/page.tsx # Installment management
│   │   │   └── layout.tsx          # Root layout
│   │   ├── lib/
│   │   │   ├── api.ts              # API client
│   │   │   └── auth.ts             # Auth context
│   │   └── globals.css
│   ├── package.json
│   └── next.config.js
└── README.md
```

## Setup Instructions

### Prerequisites
- Python 3.8+
- Node.js 16+
- PostgreSQL (or SQLite for development)
- SendGrid account (for email functionality)

### Backend Setup

1. **Navigate to backend directory:**
   ```bash
   cd backend
   ```

2. **Create virtual environment:**
   ```bash
   python -m venv venv
   
   # Windows
   venv\Scripts\activate
   
   # macOS/Linux
   source venv/bin/activate
   ```

3. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

4. **Set up environment variables:**
   ```bash
   cp .env.example .env
   ```
   
   Edit `.env` file with your configuration:
   ```env
   DATABASE_URL=postgresql://username:password@localhost/payment_reminder
   SECRET_KEY=your-secret-key-here
   ALGORITHM=HS256
   ACCESS_TOKEN_EXPIRE_MINUTES=30
   SENDGRID_API_KEY=your-sendgrid-api-key
   FROM_EMAIL=noreply@yourcompany.com
   PAYMENT_LINK=https://yourpaymentpage.com/pay
   ```

5. **Initialize database:**
   ```bash
   # The database tables will be created automatically when you run the app
   ```

6. **Run the backend server:**
   ```bash
   python main.py
   ```
   
   The API will be available at `http://localhost:8000`

### Frontend Setup

1. **Navigate to frontend directory:**
   ```bash
   cd frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Set environment variables:**
   Create `.env.local` file:
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:8000
   ```

4. **Run the frontend:**
   ```bash
   npm run dev
   ```
   
   The frontend will be available at `http://localhost:3000`

## Usage

### 1. First Time Setup

1. **Access the application:** Open `http://localhost:3000` in your browser
2. **Create admin account:** Use the API endpoint `/users/` to create your first admin user
3. **Login:** Use the login page with your admin credentials

### 2. Managing Clients

1. **Navigate to Clients page** from the dashboard
2. **Add new clients** with their information
3. **Edit or delete** existing clients as needed

### 3. Managing Installments

1. **Navigate to Installments page** from the dashboard
2. **Add new installments** for each client
3. **Set due dates** and amounts
4. **Mark payments** as received when clients pay

### 4. Using the Spreadsheet

1. **Navigate to Spreadsheet page** from the dashboard
2. **Edit cells directly** by clicking on them
3. **Toggle payment status** by clicking on the "Paid" column
4. **Add new rows** for new customers
5. **Delete rows** when needed

## Email Automation

The system automatically sends emails:

- **Daily at 9:00 AM:** Reminder emails for installments due in the next 3 days
- **Daily at 10:00 AM:** Overdue payment notices (every 7 days for overdue items)

### Email Templates

- **Reminder Email:** Friendly reminder with payment link
- **Overdue Email:** Urgent notice for overdue payments
- **Paid Confirmation:** (Can be implemented as needed)

## API Endpoints

### Authentication
- `POST /token` - Login and get access token
- `POST /users/` - Create new user
- `GET /users/me` - Get current user info

### Clients
- `GET /clients/` - List all clients
- `POST /clients/` - Create new client
- `GET /clients/{id}` - Get client details
- `PUT /clients/{id}` - Update client
- `DELETE /clients/{id}` - Delete client

### Installments
- `GET /installments/` - List all installments
- `POST /installments/` - Create new installment
- `GET /installments/{id}` - Get installment details
- `PUT /installments/{id}` - Update installment
- `PUT /installments/{id}/mark-paid` - Mark installment as paid
- `DELETE /installments/{id}` - Delete installment

### Dashboard
- `GET /dashboard/summary` - Get dashboard statistics
- `GET /dashboard/overdue-installments` - Get overdue installments
- `GET /dashboard/upcoming-installments` - Get upcoming installments

## Database Schema

### Users Table
- `id` (Primary Key)
- `username` (Unique)
- `email` (Unique)
- `hashed_password`
- `is_active`
- `created_at`

### Clients Table
- `id` (Primary Key)
- `name`
- `email`
- `phone`
- `address`
- `created_by` (Foreign Key to Users)
- `created_at`
- `updated_at`

### Installments Table
- `id` (Primary Key)
- `client_id` (Foreign Key to Clients)
- `amount`
- `due_date`
- `description`
- `is_paid`
- `paid_at`
- `reminder_sent`
- `last_reminder_sent`
- `created_at`

### Email Logs Table
- `id` (Primary Key)
- `client_id` (Foreign Key to Clients)
- `installment_id` (Foreign Key to Installments)
- `email_type`
- `sent_at`
- `status`
- `error_message`

## Development

### Running Tests

**Backend:**
```bash
cd backend
pytest
```

**Frontend:**
```bash
cd frontend
npm test
```

### Database Migrations

For production, consider using Alembic for database migrations:

```bash
cd backend
alembic init alembic
alembic revision --autogenerate -m "Initial migration"
alembic upgrade head
```

## Deployment

### Backend Deployment

1. **Use a production WSGI server** like Gunicorn:
   ```bash
   pip install gunicorn
   gunicorn -w 4 -k uvicorn.workers.UvicornWorker main:app
   ```

2. **Set up PostgreSQL** for production database

3. **Configure environment variables** in production

### Frontend Deployment

1. **Build the application:**
   ```bash
   npm run build
   ```

2. **Deploy to Vercel, Netlify, or any Node.js hosting platform**

## Security Considerations

- **JWT tokens** have configurable expiration
- **Password hashing** with bcrypt
- **CORS configuration** for API access
- **Input validation** with Pydantic schemas
- **SQL injection prevention** with SQLAlchemy ORM

## Troubleshooting

### Common Issues

1. **Database Connection Errors:**
   - Check DATABASE_URL in .env file
   - Ensure PostgreSQL is running
   - Verify database credentials

2. **Email Not Sending:**
   - Verify SendGrid API key
   - Check FROM_EMAIL configuration
   - Review email logs in database

3. **Frontend API Errors:**
   - Ensure backend is running on correct port
   - Check NEXT_PUBLIC_API_URL in frontend .env.local
   - Verify CORS settings

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review the API documentation at `http://localhost:8000/docs`
3. Create an issue in the repository
