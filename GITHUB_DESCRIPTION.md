# 📧 Payment Reminder System

A comprehensive payment reminder system that automatically sends email notifications to clients about upcoming and overdue payments. Built with FastAPI backend and Next.js frontend with a modern dark-themed spreadsheet interface.

## ✨ Features

### 🚀 Core Functionality
- **Automated Email Reminders** - Daily emails for upcoming payments and overdue notices
- **Online Spreadsheet Interface** - Editable cells for managing customer data
- **Dark Theme Support** - Beautiful dark/light mode toggle with smooth transitions
- **Real-time Payment Tracking** - Mark payments as paid instantly
- **Client Management** - Complete CRUD operations for customer data
- **Dashboard Analytics** - Overview of payment statistics and metrics

### 🎨 User Experience
- **Modern Dark Theme** - Easy on the eyes with high contrast
- **Responsive Design** - Works perfectly on desktop and mobile
- **Inline Editing** - Click any cell to edit directly in the spreadsheet
- **Email Composition** - Write and preview reminder emails before sending
- **Professional UI** - Clean, intuitive interface with Tailwind CSS

### 🔧 Technical Stack
- **Backend**: FastAPI with SQLAlchemy and PostgreSQL
- **Frontend**: Next.js with TypeScript and Tailwind CSS
- **Authentication**: JWT-based secure login system
- **Email Service**: SendGrid integration with APScheduler
- **Database**: SQLAlchemy ORM with Alembic migrations

## 📋 Spreadsheet Fields

The main spreadsheet interface includes all essential customer payment fields:
- **First Name** - Customer's first name
- **Last Name** - Customer's last name  
- **Email Address** - Contact information for reminders
- **Reminder Date** - When reminder email was sent
- **Due Date** - Payment deadline
- **Paid Status** - Visual indicator (Yes/No) with color coding

## 📧 Email Automation

The system automatically sends two types of emails:

### 📅 Daily Reminders (9:00 AM)
- Sends emails to clients with payments due in the next 3 days
- Includes payment amount, due date, and direct payment link
- Professional template with personalized greeting

### ⚠️ Overdue Notices (10:00 AM)  
- Sends urgent notices for overdue payments every 7 days
- Clear call-to-action with payment instructions
- Helps recover late payments automatically

## 🛠️ Development Features

### 🔐 Security
- JWT authentication with configurable token expiration
- Password hashing with bcrypt
- CORS configuration for API security
- Input validation with Pydantic schemas

### 📊 Dashboard Analytics
- Total clients overview
- Paid vs unpaid installment statistics
- Overdue payment tracking
- Upcoming payment notifications

### 🎯 Management Tools
- Add/edit/delete clients
- Create and manage installments
- Mark payments as paid with timestamp
- Email composition with live preview

## 🚀 Getting Started

### Prerequisites
- Python 3.8+ and Node.js 16+
- PostgreSQL database (or SQLite for development)
- SendGrid account for email functionality

### Quick Setup
```bash
# Backend
cd backend && python -m venv venv && source venv/bin/activate
pip install -r requirements.txt
python main.py

# Frontend  
cd frontend && npm install
npm run dev
```

## 📱 Screenshots

### 🌙 Dark Theme Spreadsheet
- Beautiful dark interface with high contrast
- Editable cells with inline editing
- Color-coded payment status indicators

### 📊 Dashboard Overview
- Real-time payment statistics
- Quick access to management tools
- Visual metrics and charts

### 📧 Email Composition
- Professional email templates
- Live preview functionality
- Customizable payment links

## 🔧 Configuration

The system is highly configurable through environment variables:
- Database connection settings
- Email service configuration
- JWT security parameters
- Payment customization options

## 📈 Business Benefits

### 💰 Increased Revenue
- Automated reminders reduce late payments
- Professional communication improves customer satisfaction
- Easy payment tracking prevents missed installments

### ⏰ Time Savings
- Automated email system saves hours of manual work
- Spreadsheet interface for quick data management
- One-click payment status updates

### 📊 Better Insights
- Dashboard provides clear payment overview
- Track payment trends and patterns
- Identify clients needing attention

## 🛡️ Production Ready

The system is built with production in mind:
- Scalable database architecture
- Error handling and logging
- Security best practices
- Easy deployment configuration

## 🤝 Contributing

Contributions are welcome! The codebase includes:
- Comprehensive inline comments
- Clear documentation
- Modular architecture
- Test coverage

## 📄 License

This project is open source and available under the MIT License.

---

**Perfect for businesses, freelancers, and anyone who needs to track client payments and send automated reminders!**

🌟 **Key Features**: Automated emails • Dark theme • Spreadsheet interface • Real-time updates • Secure authentication
