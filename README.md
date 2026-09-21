# Payment Reminder Application

A complete application for managing payments and automatic reminders, based on provided designs. This application includes a robust C# backend with ASP.NET Core and a modern Next.js frontend with TypeScript.

## 🚀 Technologies

### Backend (C# / ASP.NET Core)
- **ASP.NET Core 10** - Modern web framework
- **Entity Framework Core** - ORM for database
- **SQLite** - Lightweight database
- **JWT Authentication** - Secure authentication system
- **Hangfire** - Job scheduling system
- **SignalR** - Real-time notifications
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **System.IdentityModel.Tokens.Jwt** - JWT token handling

### Frontend (Next.js/React)
- **Next.js 16** - Modern React framework
- **TypeScript** - Static typing
- **Tailwind CSS** - Responsive styling
- **Axios** - HTTP client
- **SignalR** - Real-time notifications
- **Lucide React** - Modern icons
- **date-fns** - Date handling

## 📋 Features

- ✅ JWT authentication system
- ✅ Complete payment management (CRUD)
- ✅ Priority system (Urgent, High, Medium, Low)
- ✅ Payment status tracking (Pending, Paid, Overdue, etc.)
- ✅ Automatic email reminders
- ✅ Dashboard with real-time statistics
- ✅ Payment status filters
- ✅ Responsive and modern design
- ✅ Real-time notifications

## 🛠️ Installation

### Backend (C#)
```bash
cd Backend
dotnet restore
dotnet build
dotnet run
```

The backend will run on `http://localhost:5122`

**Demo Mode:** The backend includes a demo mode with pre-configured user:
- Username: `demo`
- Password: `demo123`
- Includes 4 pre-configured demo payments

### Frontend
```bash
cd frontend
npm install
npm run dev
```

The frontend will run on `http://localhost:3000`

## 🔧 Configuration

### Backend Environment Variables (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-change-this-in-production",
    "Issuer": "PaymentReminder.Api",
    "Audience": "PaymentReminder.Client",
    "ExpirationInMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@paymentreminder.com",
    "FromName": "Payment Reminder"
  }
}
```

### Frontend Environment Variables (.env)
```env
NEXT_PUBLIC_API_URL=http://localhost:5122/api
```

## 📁 Project Structure

```
Payment Reminder Application/
├── Backend/                 # Backend API (C#)
│   ├── Controllers/        # API Controllers
│   ├── Models/            # Data Models
│   ├── Services/          # Business Logic
│   ├── Data/              # Database Context
│   ├── DTOs/              # Data Transfer Objects
│   ├── Hubs/              # SignalR Hubs
│   ├── Program.cs         # Entry Point
│   └── appsettings.json   # Configuration
├── frontend/              # Frontend Next.js
│   ├── src/
│   │   ├── app/          # Next.js Pages
│   │   ├── components/   # React Components
│   │   ├── context/     # Contexts (Auth)
│   │   └── lib/         # Utilities and API
│   └── package.json
└── README.md
```

## 🔗 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user

### Payments
- `GET /api/payments` - Get all user payments
- `GET /api/payments/:id` - Get specific payment
- `POST /api/payments` - Create new payment
- `PUT /api/payments/:id` - Update payment
- `DELETE /api/payments/:id` - Delete payment
- `POST /api/payments/:id/mark-paid` - Mark payment as paid
- `GET /api/payments/overdue` - Get overdue payments

## 🎨 Design

The application follows a modern design with:
- Blue/gray colors as in reference images
- Smooth gradients
- Cards with shadows
- Intuitive icons
- Responsive layout
- Subtle animations

## 📅 Reminder System

The system sends automatic reminders at these intervals:
- 1 week before due date
- 3 days before due date
- 1 day before due date
- 1 day after due date (if overdue)

Reminders are processed automatically with **Hangfire**, a robust job scheduling system for .NET.

## 🔐 Security

- Passwords encrypted with ASP.NET Core Identity
- JWT tokens for authentication with configurable expiration
- Data validation on both sides
- CORS configured for frontend
- Environment variables for sensitive data
- Role-based authorization (Admin/User)
- HTTPS configured for production

## 🚀 Deployment

### Backend (ASP.NET Core)
You can deploy the backend on services like:
- Azure App Service (recommended for ASP.NET Core)
- AWS App Runner
- Google Cloud Run
- Heroku
- Your own VPS with Docker

### Frontend
You can deploy the frontend on:
- Vercel (recommended for Next.js)
- Netlify
- Azure Static Web Apps
- AWS Amplify

## 📝 Notes

- SQLite database is created automatically on first run
- Email requires real SMTP configuration
- For production, change JWT_SECRET and use secure environment variables
- Reminder system uses Hangfire for scheduled jobs
- Demo mode includes pre-configured data for easy testing
- Application uses Entity Framework Core for data access

## 🤝 Contribution

This project was created as a demonstration based on provided designs. You can extend it with:
- Push notification system
- Data export to PDF/Excel
- Payment gateway integration
- Admin dashboard
- Multi-tenant system

## 📄 License

This project is for educational and demonstration purposes.