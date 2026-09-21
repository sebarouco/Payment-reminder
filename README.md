# Payment Reminder Application

Una aplicación completa para gestionar pagos y recordatorios automáticos, basada en los diseños proporcionados. Esta aplicación incluye un backend robusto en C# con ASP.NET Core y un frontend moderno en Next.js con TypeScript.

## 🚀 Tecnologías

### Backend (C# / ASP.NET Core)
- **ASP.NET Core 10** - Framework web moderno
- **Entity Framework Core** - ORM para base de datos
- **SQLite** - Base de datos ligera
- **JWT Authentication** - Sistema de autenticación seguro
- **Hangfire** - Sistema de jobs programados
- **SignalR** - Notificaciones en tiempo real
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Autenticación JWT
- **System.IdentityModel.Tokens.Jwt** - Manejo de tokens JWT

### Frontend (Next.js/React)
- **Next.js 16** - Framework React moderno
- **TypeScript** - Tipado estático
- **Tailwind CSS** - Estilos responsive
- **Axios** - Cliente HTTP
- **SignalR** - Notificaciones en tiempo real
- **Lucide React** - Iconos modernos
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

### Backend (C#)
```bash
cd Backend
dotnet restore
dotnet build
dotnet run
```

El backend se ejecutará en `http://localhost:5122`

**Modo Demo:** El backend incluye un modo demo con usuario pre-configurado:
- Usuario: `demo`
- Contraseña: `demo123`
- Incluye 4 pagos de demostración pre-configurados

### Frontend
```bash
cd frontend
npm install
npm run dev
```

El frontend se ejecutará en `http://localhost:3000`

## 🔧 Configuración

### Variables de entorno del Backend (appsettings.json)
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

### Variables de entorno del Frontend (.env)
```env
NEXT_PUBLIC_API_URL=http://localhost:5122/api
```

## 📁 Estructura del Proyecto

```
Payment Reminder Application/
├── Backend/                 # Backend API (C#)
│   ├── Controllers/        # Controladores API
│   ├── Models/            # Modelos de datos
│   ├── Services/          # Lógica de negocio
│   ├── Data/              # Contexto de base de datos
│   ├── DTOs/              # Data Transfer Objects
│   ├── Hubs/              # SignalR Hubs
│   ├── Program.cs         # Punto de entrada
│   └── appsettings.json   # Configuración
├── frontend/              # Frontend Next.js
│   ├── src/
│   │   ├── app/          # Páginas Next.js
│   │   ├── components/   # Componentes React
│   │   ├── context/     # Contextos (Auth)
│   │   └── lib/         # Utilidades y API
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

Los recordatorios se procesan automáticamente con **Hangfire**, un sistema robusto de jobs programados para .NET.

## 🔐 Seguridad

- Contraseñas encriptadas con ASP.NET Core Identity
- Tokens JWT para autenticación con expiración configurable
- Validación de datos en ambos lados
- CORS configurado para el frontend
- Variables de entorno para datos sensibles
- Role-based authorization (Admin/User)
- HTTPS configurado para producción

## 🚀 Despliegue

### Backend (ASP.NET Core)
Puedes desplegar el backend en servicios como:
- Azure App Service (recomendado para ASP.NET Core)
- AWS App Runner
- Google Cloud Run
- Heroku
- VPS propio con Docker

### Frontend
Puedes desplegar el frontend en:
- Vercel (recomendado para Next.js)
- Netlify
- Azure Static Web Apps
- AWS Amplify

## 📝 Notas

- La base de datos SQLite se crea automáticamente en la primera ejecución
- Los correos electrónicos requieren configuración SMTP real
- Para producción, cambia el JWT_SECRET y usa variables de entorno seguras
- El sistema de recordatorios usa Hangfire para jobs programados
- El modo demo incluye datos pre-configurados para facilitar pruebas
- La aplicación usa Entity Framework Core para el acceso a datos

## 🤝 Contribución

Este proyecto fue creado como una demostración basada en diseños proporcionados. Puedes extenderlo con:
- Sistema de notificaciones push
- Exportación de datos a PDF/Excel
- Integración con pasarelas de pago
- Dashboard administrativo
- Sistema de multi-tenant

## 📄 Licencia

Este proyecto es para fines educativos y demostrativos.