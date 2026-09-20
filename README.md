# Payment Reminder Application

Una aplicación completa para gestionar pagos y recordatorios automáticos, basada en los diseños proporcionados.

## 🚀 Tecnologías

### Backend (Node.js/Express)
- **Express.js** - Framework web
- **SQLite (better-sqlite3)** - Base de datos
- **JWT** - Autenticación
- **bcryptjs** - Encriptación de contraseñas
- **Nodemailer** - Envío de correos electrónicos
- **dotenv** - Gestión de variables de entorno

### Frontend (Next.js/React)
- **Next.js 16** - Framework React
- **TypeScript** - Tipado estático
- **Tailwind CSS** - Estilos
- **Axios** - Cliente HTTP
- **SignalR** - Notificaciones en tiempo real
- **Lucide React** - Iconos
- **date-fns** - Manejo de fechas

## 📋 Características

- ✅ Sistema de autenticación con JWT
- ✅ Gestión completa de pagos (CRUD)
- ✅ Sistema de prioridades (Urgent, High, Medium, Low)
- ✅ Estados de pago (Pending, Paid, Overdue, etc.)
- ✅ Recordatorios automáticos por email
- ✅ Dashboard con estadísticas en tiempo real
- ✅ Filtros por estado de pago
- ✅ Diseño responsive y moderno
- ✅ Notificaciones en tiempo real

## 🛠️ Instalación

### Backend
```bash
cd Backend-Node
npm install
npm start
```

El backend se ejecutará en `http://localhost:5000`

### Frontend
```bash
cd frontend
npm install
npm run dev
```

El frontend se ejecutará en `http://localhost:3000`

## 🔧 Configuración

### Variables de entorno del Backend (.env)
```env
PORT=5000
JWT_SECRET=your-secret-key-change-this-in-production
JWT_EXPIRATION=60m
DB_PATH=./paymentreminder.db
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM=noreply@paymentreminder.com
```

### Variables de entorno del Frontend (.env)
```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

## 📁 Estructura del Proyecto

```
Payment Reminder Application/
├── Backend-Node/              # Backend API
│   ├── server.js            # Servidor Express
│   ├── package.json         # Dependencias
│   └── .env                 # Variables de entorno
├── frontend/                # Frontend Next.js
│   ├── src/
│   │   ├── app/            # Páginas Next.js
│   │   ├── components/     # Componentes React
│   │   ├── context/       # Contextos (Auth)
│   │   └── lib/           # Utilidades y API
│   └── package.json
└── README.md
```

## 🔗 API Endpoints

### Autenticación
- `POST /api/auth/register` - Registro de usuarios
- `POST /api/auth/login` - Login de usuarios
- `GET /api/auth/me` - Obtener usuario actual

### Pagos
- `GET /api/payments` - Obtener todos los pagos del usuario
- `GET /api/payments/:id` - Obtener un pago específico
- `POST /api/payments` - Crear nuevo pago
- `PUT /api/payments/:id` - Actualizar pago
- `DELETE /api/payments/:id` - Eliminar pago
- `POST /api/payments/:id/mark-paid` - Marcar pago como realizado
- `GET /api/payments/overdue` - Obtener pagos vencidos

## 🎨 Diseño

La aplicación sigue un diseño moderno con:
- Colores azul/gris como en las imágenes de referencia
- Gradientes suaves
- Tarjetas con sombras
- Iconos intuitivos
- Layout responsive
- Animaciones sutiles

## 📅 Sistema de Recordatorios

El sistema envía recordatorios automáticos en estos intervalos:
- 1 semana antes de la fecha de vencimiento
- 3 días antes de la fecha de vencimiento
- 1 día antes de la fecha de vencimiento
- 1 día después de la fecha de vencimiento (si está vencido)

Los recordatorios se procesan cada minuto y los pagos vencidos se actualizan cada hora.

## 🔐 Seguridad

- Contraseñas encriptadas con bcrypt
- Tokens JWT para autenticación
- Validación de datos en ambos lados
- CORS configurado
- Variables de entorno para datos sensibles

## 🚀 Despliegue

### Backend
Puedes desplegar el backend en servicios como:
- Railway
- Render
- Heroku
- VPS propio

### Frontend
Puedes desplegar el frontend en:
- Vercel (recomendado para Next.js)
- Netlify
- Railway

## 📝 Notas

- La base de datos SQLite se crea automáticamente en la primera ejecución
- Los correos electrónicos requieren configuración SMTP real
- Para producción, cambia el JWT_SECRET y usa variables de entorno seguras
- El sistema de recordatorios usa jobs programados con setInterval

## 🤝 Contribución

Este proyecto fue creado como una demostración basada en diseños proporcionados. Puedes extenderlo con:
- Sistema de notificaciones push
- Exportación de datos a PDF/Excel
- Integración con pasarelas de pago
- Dashboard administrativo
- Sistema de multi-tenant

## 📄 Licencia

Este proyecto es para fines educativos y demostrativos.