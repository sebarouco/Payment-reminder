# Payment Reminder Application - Demo Mode

Esta aplicación ahora incluye un **modo demo** que permite probar todas las funcionalidades sin necesidad de registro real.

## 🚀 Cómo Ejecutar

### 1. Publicar la aplicación
```bash
cd "/home/sugar/Escritorios/Programacion/Programas convertidos a c sharp y c mas mas/Payment Reminder Application/Backend"
dotnet publish -c Release -r linux-x64 --self-contained
```

### 2. Ejecutar el servidor
```bash
cd bin/Release/net10.0/linux-x64/publish
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5122" ./PaymentReminder.Api
```

## 🔑 Credenciales Demo

La aplicación crea automáticamente un usuario demo con las siguientes credenciales:

- **Usuario**: `demo`
- **Contraseña**: `demo123`
- **Rol**: Admin (permite acceso a todas las funciones)

## 📊 Datos de Demostración

El usuario demo incluye 4 pagos pre-configurados:

1. **Acme Corporation** - $1,500.00 (Vence en 7 días, Alta prioridad)
2. **Tech Solutions Inc** - $3,200.50 (Vence en 14 días, Prioridad media)
3. **Global Services Ltd** - $850.00 (Vencido hace 2 días, Prioridad urgente)
4. **StartUp Ventures** - $5,000.00 (Pagado, Alta prioridad)

## 🧪 Probando la Aplicación

### Obtener credenciales demo
```bash
curl http://localhost:5122/api/auth/demo
```

### Iniciar sesión
```bash
curl -X POST http://localhost:5122/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","password":"demo123"}'
```

### Obtener pagos del usuario
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5122/api/payments
```

### Obtener pagos vencidos (solo admin)
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5122/api/payments/overdue
```

### Crear nuevo pago
```bash
curl -X POST http://localhost:5122/api/payments \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientName": "New Client",
    "clientEmail": "client@example.com",
    "clientPhone": "+1-555-9999",
    "amount": 2500.00,
    "currency": "USD",
    "description": "New project services",
    "dueDate": "2024-10-15T00:00:00Z",
    "priority": "High"
  }'
```

## 🎯 Funcionalidades Disponibles

### Autenticación
- ✅ Registro de usuarios (disponible pero no necesario en modo demo)
- ✅ Login con JWT
- ✅ Obtener información del usuario actual
- ✅ Endpoint de credenciales demo

### Gestión de Pagos
- ✅ Crear pagos
- ✅ Listar pagos del usuario
- ✅ Ver detalles de pago específico
- ✅ Actualizar pagos
- ✅ Eliminar pagos
- ✅ Marcar pagos como pagados
- ✅ Ver pagos vencidos (admin)

### Sistema de Recordatorios
- ✅ Crear recordatorios manuales
- ✅ Programar recordatorios automáticos
- ✅ Ver recordatorios de un pago
- ✅ Procesar recordatorios
- ✅ Jobs automáticos con Hangfire

### Características Técnicas
- ✅ Base de datos SQLite
- ✅ Autenticación JWT con roles
- ✅ CORS configurado para frontend
- ✅ Logging estructurado
- ✅ Email con fallback (no requiere configuración)
- ✅ Validación de datos
- ✅ Manejo de errores robusto

## 🌐 Endpoints Principales

### Autenticación
- `GET /api/auth/demo` - Obtener credenciales demo
- `POST /api/auth/register` - Registrar nuevo usuario
- `POST /api/auth/login` - Iniciar sesión
- `GET /api/auth/me` - Obtener usuario actual

### Pagos
- `GET /api/payments` - Listar pagos del usuario
- `GET /api/payments/{id}` - Obtener pago específico
- `POST /api/payments` - Crear nuevo pago
- `PUT /api/payments/{id}` - Actualizar pago
- `DELETE /api/payments/{id}` - Eliminar pago
- `POST /api/payments/{id}/mark-paid` - Marcar como pagado
- `GET /api/payments/overdue` - Ver pagos vencidos (admin)

### Recordatorios
- `GET /api/reminders/payment/{paymentId}` - Ver recordatorios de pago
- `POST /api/reminders` - Crear recordatorio
- `POST /api/reminders/{id}/process` - Procesar recordatorio
- `POST /api/reminders/payment/{paymentId}/schedule` - Programar recordatorios

## 🔧 Configuración

### Email (Opcional)
Para habilitar el envío real de emails, actualiza `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "tu-email@gmail.com",
  "SmtpPassword": "tu-app-password",
  "FromEmail": "noreply@paymentreminder.com",
  "FromName": "Payment Reminder"
}
```

Sin configuración, la aplicación funcionará normalmente y los emails se registrarán en los logs.

## 📝 Notas

- La base de datos se crea automáticamente en `paymentreminder.db`
- Los datos demo se crean automáticamente al iniciar la aplicación
- El usuario demo tiene rol Admin para probar todas las funcionalidades
- Los emails no requieren configuración (usará fallback de logging)
- Hangfire ejecuta jobs automáticamente para procesar recordatorios

## 🎉 Ventajas del Modo Demo

1. **Sin configuración de email** - Funciona inmediatamente
2. **Datos pre-cargados** - No necesitas crear datos manualmente
3. **Prueba completa** - Todos los endpoints funcionan
4. **Ideal para demostraciones** - Muestra todas las capacidades
5. **Fácil de resetear** - Solo borra el archivo `paymentreminder.db`

## 🔄 Resetear Demo

Para volver a crear los datos demo:

```bash
rm paymentreminder.db
# Reinicia la aplicación
./PaymentReminder.Api
```

La aplicación creará automáticamente un nuevo usuario demo con los datos de prueba.