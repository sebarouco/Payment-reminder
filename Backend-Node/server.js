const express = require('express');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');
const Database = require('better-sqlite3');
const cors = require('cors');
const nodemailer = require('nodemailer');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 5000;

// Middleware
app.use(cors());
app.use(express.json());

// Database setup
const db = new Database(process.env.DB_PATH || './paymentreminder.db');

// Initialize database tables
db.exec(`CREATE TABLE IF NOT EXISTS users (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  username TEXT UNIQUE NOT NULL,
  email TEXT UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  last_login DATETIME
)`);

db.exec(`CREATE TABLE IF NOT EXISTS payments (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  user_id INTEGER NOT NULL,
  client_name TEXT NOT NULL,
  client_email TEXT NOT NULL,
  client_phone TEXT,
  amount REAL NOT NULL,
  currency TEXT DEFAULT 'USD',
  description TEXT,
  due_date DATETIME NOT NULL,
  paid_date DATETIME,
  status TEXT DEFAULT 'Pending',
  priority TEXT DEFAULT 'Medium',
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES users(id)
)`);

db.exec(`CREATE TABLE IF NOT EXISTS reminders (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  payment_id INTEGER NOT NULL,
  scheduled_date DATETIME NOT NULL,
  sent_date DATETIME,
  status TEXT DEFAULT 'Scheduled',
  method TEXT DEFAULT 'Email',
  message TEXT,
  attempts INTEGER DEFAULT 0,
  error TEXT,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (payment_id) REFERENCES payments(id)
)`);

// Email configuration
const transporter = nodemailer.createTransport({
  host: process.env.SMTP_HOST || 'smtp.gmail.com',
  port: process.env.SMTP_PORT || 587,
  secure: false,
  auth: {
    user: process.env.SMTP_USER,
    pass: process.env.SMTP_PASSWORD
  }
});

// JWT middleware
const authenticateToken = (req, res, next) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];

  if (!token) {
    return res.status(401).json({ error: 'Access token required' });
  }

  jwt.verify(token, process.env.JWT_SECRET, (err, user) => {
    if (err) {
      return res.status(403).json({ error: 'Invalid token' });
    }
    req.user = user;
    next();
  });
};

// Auth routes
app.post('/api/auth/register', async (req, res) => {
  const { username, email, password } = req.body;

  if (!username || !email || !password) {
    return res.status(400).json({ error: 'All fields are required' });
  }

  try {
    const hashedPassword = await bcrypt.hash(password, 10);
    
    try {
      const result = db.prepare(
        'INSERT INTO users (username, email, password_hash) VALUES (?, ?, ?)'
      ).run(username, email, hashedPassword);

      const token = jwt.sign(
        { id: result.lastInsertRowid, username, email },
        process.env.JWT_SECRET,
        { expiresIn: process.env.JWT_EXPIRATION || '60m' }
      );

      res.json({ token });
    } catch (dbError) {
      if (dbError.message.includes('UNIQUE constraint failed')) {
        return res.status(400).json({ error: 'Username or email already exists' });
      }
      return res.status(500).json({ error: 'Database error' });
    }
  } catch (error) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.post('/api/auth/login', async (req, res) => {
  const { username, password } = req.body;

  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password are required' });
  }

  try {
    const user = db.prepare(
      'SELECT * FROM users WHERE username = ? OR email = ?'
    ).get(username, username);

    if (!user) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    const validPassword = await bcrypt.compare(password, user.password_hash);
    if (!validPassword) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    // Update last login
    db.prepare('UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE id = ?').run(user.id);

    const token = jwt.sign(
      { id: user.id, username: user.username, email: user.email },
      process.env.JWT_SECRET,
      { expiresIn: process.env.JWT_EXPIRATION || '60m' }
    );

    res.json({ token });
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/auth/me', authenticateToken, (req, res) => {
  try {
    const user = db.prepare(
      'SELECT id, username, email, created_at, last_login FROM users WHERE id = ?'
    ).get(req.user.id);

    if (!user) {
      return res.status(404).json({ error: 'User not found' });
    }
    res.json(user);
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

// Payment routes
app.get('/api/payments', authenticateToken, (req, res) => {
  try {
    const payments = db.prepare(
      'SELECT * FROM payments WHERE user_id = ? ORDER BY due_date ASC'
    ).all(req.user.id);
    res.json(payments);
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/payments/:id', authenticateToken, (req, res) => {
  try {
    const payment = db.prepare(
      'SELECT * FROM payments WHERE id = ? AND user_id = ?'
    ).get(req.params.id, req.user.id);

    if (!payment) {
      return res.status(404).json({ error: 'Payment not found' });
    }
    res.json(payment);
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.post('/api/payments', authenticateToken, (req, res) => {
  const { client_name, client_email, client_phone, amount, currency, description, due_date, priority } = req.body;

  if (!client_name || !client_email || !amount || !description || !due_date) {
    return res.status(400).json({ error: 'Required fields are missing' });
  }

  try {
    const result = db.prepare(
      `INSERT INTO payments (user_id, client_name, client_email, client_phone, amount, currency, description, due_date, priority)
       VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`
    ).run(req.user.id, client_name, client_email, client_phone, amount, currency || 'USD', description, due_date, priority || 'Medium');

    // Schedule reminders
    scheduleReminders(result.lastInsertRowid, due_date);

    res.status(201).json({ id: result.lastInsertRowid, ...req.body, user_id: req.user.id });
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.put('/api/payments/:id', authenticateToken, (req, res) => {
  const { client_name, client_email, client_phone, amount, currency, description, due_date, priority, status } = req.body;

  try {
    const result = db.prepare(
      `UPDATE payments 
       SET client_name = ?, client_email = ?, client_phone = ?, amount = ?, currency = ?, 
           description = ?, due_date = ?, priority = ?, status = ?, updated_at = CURRENT_TIMESTAMP
       WHERE id = ? AND user_id = ?`
    ).run(client_name, client_email, client_phone, amount, currency, description, due_date, priority, status, req.params.id, req.user.id);

    if (result.changes === 0) {
      return res.status(404).json({ error: 'Payment not found' });
    }
    res.json({ id: req.params.id, ...req.body });
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.delete('/api/payments/:id', authenticateToken, (req, res) => {
  try {
    const result = db.prepare(
      'DELETE FROM payments WHERE id = ? AND user_id = ?'
    ).run(req.params.id, req.user.id);

    if (result.changes === 0) {
      return res.status(404).json({ error: 'Payment not found' });
    }
    res.status(204).send();
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.post('/api/payments/:id/mark-paid', authenticateToken, (req, res) => {
  try {
    const result = db.prepare(
      "UPDATE payments SET status = 'Paid', paid_date = CURRENT_TIMESTAMP, updated_at = CURRENT_TIMESTAMP WHERE id = ? AND user_id = ?"
    ).run(req.params.id, req.user.id);

    if (result.changes === 0) {
      return res.status(404).json({ error: 'Payment not found' });
    }
    res.json({ message: 'Payment marked as paid' });
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

app.get('/api/payments/overdue', authenticateToken, (req, res) => {
  try {
    const payments = db.prepare(
      "SELECT * FROM payments WHERE due_date < CURRENT_TIMESTAMP AND status != 'Paid'"
    ).all();
    res.json(payments);
  } catch (error) {
    res.status(500).json({ error: 'Database error' });
  }
});

// Helper function to schedule reminders
function scheduleReminders(paymentId, dueDate) {
  const reminderDates = [
    new Date(new Date(dueDate).getTime() - 7 * 24 * 60 * 60 * 1000), // 1 week before
    new Date(new Date(dueDate).getTime() - 3 * 24 * 60 * 60 * 1000), // 3 days before
    new Date(new Date(dueDate).getTime() - 1 * 24 * 60 * 60 * 1000), // 1 day before
    new Date(new Date(dueDate).getTime() + 1 * 24 * 60 * 60 * 1000)  // 1 day after
  ];

  reminderDates.forEach(date => {
    if (date > new Date()) {
      try {
        db.prepare(
          'INSERT INTO reminders (payment_id, scheduled_date, status, method) VALUES (?, ?, "Scheduled", "Email")'
        ).run(paymentId, date.toISOString());
      } catch (error) {
        console.error('Failed to schedule reminder:', error);
      }
    }
  });
}

// Reminder processing job (runs every minute)
setInterval(async () => {
  try {
    const reminders = db.prepare(
      "SELECT r.*, p.*, u.email as user_email FROM reminders r JOIN payments p ON r.payment_id = p.id JOIN users u ON p.user_id = u.id WHERE r.status = 'Scheduled' AND r.scheduled_date <= CURRENT_TIMESTAMP"
    ).all();

    for (const reminder of reminders) {
      try {
        // Send email reminder
        const mailOptions = {
          from: process.env.SMTP_FROM || 'noreply@paymentreminder.com',
          to: reminder.user_email,
          subject: `Payment Reminder: ${reminder.client_name} - $${reminder.amount}`,
          text: `Dear User,\n\nThis is a friendly reminder about your payment:\n\nClient: ${reminder.client_name}\nAmount: $${reminder.amount}\nDue Date: ${new Date(reminder.due_date).toLocaleDateString()}\nDescription: ${reminder.description}\n\nPlease ensure your payment is made by the due date.\n\nBest regards,\nPayment Reminder`
        };

        await transporter.sendMail(mailOptions);

        db.prepare(
          "UPDATE reminders SET status = 'Sent', sent_date = CURRENT_TIMESTAMP, attempts = attempts + 1 WHERE id = ?"
        ).run(reminder.id);
      } catch (error) {
        console.error('Failed to send reminder:', error);
        db.prepare(
          "UPDATE reminders SET status = 'Failed', error = ?, attempts = attempts + 1 WHERE id = ?"
        ).run(error.message, reminder.id);
      }
    }
  } catch (error) {
    console.error('Error processing reminders:', error);
  }
}, 60000); // Run every minute

// Update overdue payments job (runs every hour)
setInterval(() => {
  try {
    db.prepare(
      "UPDATE payments SET status = 'Overdue', updated_at = CURRENT_TIMESTAMP WHERE due_date < CURRENT_TIMESTAMP AND status = 'Pending'"
    ).run();
  } catch (error) {
    console.error('Error updating overdue payments:', error);
  }
}, 3600000); // Run every hour

// Start server
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
  console.log(`Database: ${process.env.DB_PATH || './paymentreminder.db'}`);
});