using System.Security.Claims;
using System.Text;
using Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Servicios.Interfaces;
using Servicios.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Capa de datos (base de datos en memoria). El registro vive en el proyecto Datos;
// el API solo invoca la extensión y nunca manipula el DbContext.
builder.Services.AgregarDatos();

// Inyección de dependencias: cada interfaz se resuelve con su servicio.
builder.Services.AddTransient<IAutenticacion, ServicioAutenticacion>();
builder.Services.AddTransient<IEstudiantes, ServicioEstudiantes>();
builder.Services.AddTransient<IResultados, ServicioResultados>();

var claveJwt = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException("No se configuró Jwt:Clave.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = true;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Crea la base de datos en memoria al arrancar.
app.Services.InicializarDatos();

app.UseSwagger();
app.UseSwaggerUI();

// Sirve el front end (carpeta wwwroot) desde el mismo servidor que la API.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
