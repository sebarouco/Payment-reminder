using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.MemoryStorage;
using PaymentReminder.Api.Data;
using PaymentReminder.Api.Hubs;
using PaymentReminder.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());

builder.Services.AddHangfireServer();

// SignalR
builder.Services.AddSignalR();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ReminderJobService>();
builder.Services.AddScoped<DemoUserService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Fallback policy for development
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard();

// SignalR
app.MapHub<NotificationHub>("/hubs/notifications");

// Database migration (optional - comment out if you want to handle migrations manually)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    
    // Create demo user if not exists
    var demoUserService = scope.ServiceProvider.GetRequiredService<PaymentReminder.Api.Services.DemoUserService>();
    await demoUserService.CreateDemoUserIfNotExists();
    
    // Schedule Hangfire recurring jobs
    PaymentReminder.Api.Services.HangfireJobScheduler.ScheduleRecurringJobs(scope.ServiceProvider);
}

app.Run();
