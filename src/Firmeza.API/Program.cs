// =================================================================================================
// ARCHIVO: Program.cs
// CAPA: Firmeza.API (Capa de Presentación / Entrada HTTP)
// PROPÓSITO:
//   Punto de entrada principal (Host) de la aplicación ASP.NET Core (.NET 10).
//   Configura la inyección de dependencias, autenticación JWT, Swagger UI, CORS,
//   el middleware de excepciones global y la canalización de solicitudes HTTP (Request Pipeline).
//
// ORIGEN DE DATOS Y CONFIGURACIÓN:
//   - Variables y Secretos: 'appsettings.json' y 'appsettings.Development.json' (Cadena de conexión PostgreSQL y llaves JWT).
//   - Base de Datos: PostgreSQL mediante Entity Framework Core (registrado en Firmeza.Infrastructure).
// =================================================================================================

using System.Text;
using Firmeza.API.Middleware;
using Firmeza.Application;
using Firmeza.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------------------------------
// 1. INYECCIÓN DE DEPENDENCIAS MODULAR (Clean Architecture)
//    - AddApplicationServices: Registra interfaces y casos de uso en memoria.
//    - AddInfrastructureServices: Conecta con PostgreSQL (ApplicationDbContext), Identity, EPPlus e iText.
//      (Origen de datos: Lee 'ConnectionStrings:DefaultConnection' desde appsettings.json).
// -------------------------------------------------------------------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// -------------------------------------------------------------------------------------------------
// 2. REGISTRO DE CONTROLADORES RESTful
//    Permite mapear las clases en Controllers/ a rutas HTTP (/api/Products, /api/Sales, etc.).
// -------------------------------------------------------------------------------------------------
builder.Services.AddControllers();

// -------------------------------------------------------------------------------------------------
// 3. POLÍTICA DE CORS (Cross-Origin Resource Sharing)
//    Configura acceso estricto para producción y acceso libre para desarrollo local.
// -------------------------------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Necesario si se usan cookies o autenticación delegada
    });
});

// -------------------------------------------------------------------------------------------------
// 4. AUTENTICACIÓN Y SEGURIDAD JWT (JSON Web Tokens)
//    Origen de datos: Lee sección "Jwt" en appsettings.json (Llave secreta, Emisor y Audiencia).
//    Valida la firma criptográfica en el encabezado HTTP: 'Authorization: Bearer <token>'
// -------------------------------------------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? "FirmezaSuperSecretSecurityKey2026!*SecureKeyGreaterThan32Chars";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Solo exige HTTPS en producción. En desarrollo local permite HTTP para facilitar pruebas.
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "FirmezaAPI",
        ValidAudience = jwtSettings["Audience"] ?? "FirmezaClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// -------------------------------------------------------------------------------------------------
// 5. DOCUMENTACIÓN INTERACTIVA OPENAPI / SWAGGER UI
//    Configura la interfaz gráfica en /swagger para explorar y probar endpoints,
//    incluyendo el botón verde 'Authorize' para autenticarse con el token Bearer.
// -------------------------------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Firmeza API",
        Version = "v1",
        Description = "API RESTful integral para Plataforma Firmeza: Clientes, Productos, Ventas, Excel (EPPlus), Facturación PDF (iText9) y Métricas de Dashboard."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT generado en /api/auth/login. Formato: Bearer {token}"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

var app = builder.Build();

// -------------------------------------------------------------------------------------------------
// 6. MIDDLEWARE DE MANEJO GLOBAL DE EXCEPCIONES (RFC 7807 ProblemDetails)
//    Captura centralizada de excepciones de Dominio (StockInsuficiente, ValorInvalido, ReglaNegocio)
//    y de Aplicación (NotFound, Conflict, Validation, Forbidden), traduciéndolas en respuestas
//    HTTP estructuradas (400, 403, 404, 409, 422, 500) sin ensuciar los controladores.
// -------------------------------------------------------------------------------------------------
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// -------------------------------------------------------------------------------------------------
// 7. PIPELINE DE SOLICITUDES HTTP (Middleware Pipeline)
// -------------------------------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmeza API v1");
        c.RoutePrefix = "swagger";
    });

    // Documentación interactiva moderna con Scalar (/scalar/v1)
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        options.WithTitle("Firmeza API - Scalar");
        options.WithTheme(ScalarTheme.Moon);
    });

    // Habilitar CORS permisivo solo en Desarrollo
    app.UseCors("AllowAll");
}
else
{
    // Habilitar HSTS en Producción (evita ataques de degradación a HTTP)
    app.UseHsts();
    
    // Habilitar CORS estricto en Producción
    app.UseCors("ProductionPolicy");
}

// Redirigir siempre todo el tráfico a HTTPS de forma segura
app.UseHttpsRedirection();

// Habilitar servicio de archivos estáticos (wwwroot/recibos)
app.UseStaticFiles();

// Habilitar Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de diagnóstico rápido / estado en raíz (GET /)
app.MapGet("/", () => Results.Ok(new
{
    name = "Firmeza API",
    tagline = "Plataforma Integral de Gestión Comercial, Logística y Alquiler de Maquinaria para Construcción",
    version = "1.0.0",
    status = "Online",
    environment = app.Environment.EnvironmentName,
    architecture = "Clean Architecture (.NET 10 / C# 13)",
    documentation = new
    {
        swagger = "/swagger",
        scalar = "/scalar/v1",
        openApiJson = "/swagger/v1/swagger.json"
    },
    company = new
    {
        name = "Firmeza S.A.S.",
        sector = "Construcción y Ferretería Industrial",
        country = "Colombia",
        taxId = "NIT: 900.543.210-9",
        supportEmail = "contacto@firmeza.com"
    },
    businessServices = new[]
    {
        "Venta y Distribución Mayorista de Materiales de Construcción y Ferretería",
        "Logística y Fletes con Despacho Directo a Pie de Obra",
        "Alquiler de Vehículos de Carga y Maquinaria Pesada para Constructores (US 4)",
        "Control, Trazabilidad y Auditoría de Inventario Kardex en Tiempo Real",
        "Facturación Comercial con Cálculo Automatizado de IVA (19%) y Emisión de Recibos PDF",
        "Notificaciones y Envío de Facturas por Correo Electrónico (Servicio SMTP)",
        "Importación y Exportación Masiva en Formato Excel Normalizado y Desnormalizado (EPPlus 8)"
    },
    database = new
    {
        engine = "PostgreSQL 14+",
        databaseName = "firmeza_db",
        totalTables = 18,
        securityAndIdentityTables = 7,
        commercialAndLogisticsTables = 11,
        tableList = new[]
        {
            "AspNetRoles",
            "AspNetUsers",
            "AspNetRoleClaims",
            "AspNetUserClaims",
            "AspNetUserLogins",
            "AspNetUserRoles",
            "AspNetUserTokens",
            "categorias",
            "proveedores",
            "metodos_pago",
            "clientes",
            "productos",
            "ventas",
            "detalles_venta",
            "despachos",
            "movimientos_inventario",
            "vehiculos",
            "alquileres"
        }
    },
    modules = new object[]
    {
        new { name = "Auth", route = "/api/Auth", description = "Autenticación JWT, emisión de tokens y registro de clientes" },
        new { name = "Products", route = "/api/Products", description = "Catálogo ferretero, stock e importación/exportación masiva Excel" },
        new { name = "Categories", route = "/api/Categories", description = "Familias y clasificación técnica de materiales" },
        new { name = "Suppliers", route = "/api/Suppliers", description = "Gestión de proveedores, NIT y fabricantes aliados" },
        new { name = "PaymentMethods", route = "/api/PaymentMethods", description = "Medios de recaudo comercial (Efectivo, Transferencia, Tarjeta, Crédito 30 días)" },
        new { name = "Clients", route = "/api/Clients", description = "Cartera comercial con documento único, edad y enlace a usuarios del sistema" },
        new { name = "Sales", route = "/api/Sales", description = "Facturación comercial, cálculo de IVA 19%, comprobantes PDF y anulación" },
        new { name = "Dispatches", route = "/api/Dispatches", description = "Logística, fletes y trazabilidad de despachos a obras de construcción" },
        new { name = "Vehicles", route = "/api/Vehicles", description = "Flota automotor y maquinaria pesada con tarifas diarias y capacidades de carga" },
        new { name = "Rentals", route = "/api/Rentals", description = "Contratos de alquiler de vehículos y maquinaria a clientes (US 4)" },
        new { name = "InventoryMovements", route = "/api/InventoryMovements", description = "Auditoría Kardex (entradas, salidas y ajustes en tiempo real)" },
        new { name = "Dashboard", route = "/api/Dashboard", description = "Métricas comerciales, productos bajo stock y top más vendidos" },
        new { name = "Users", route = "/api/Users", description = "Administración de usuarios y roles del sistema (Identity)" }
    },
    endpoints = new[]
    {
        "/api/Auth",
        "/api/Products",
        "/api/Categories",
        "/api/Suppliers",
        "/api/PaymentMethods",
        "/api/Clients",
        "/api/Sales",
        "/api/Dispatches",
        "/api/Vehicles",
        "/api/Rentals",
        "/api/InventoryMovements",
        "/api/Dashboard",
        "/api/Users"
    },
    timestamp = DateTime.UtcNow
})).WithName("RootWelcome");

// Endpoint amigable para /api: redirige a Swagger en navegador o devuelve índice de endpoints
app.MapGet("/api", (HttpContext context) =>
{
    var accept = context.Request.Headers.Accept.ToString();
    if (accept.Contains("text/html"))
    {
        return Results.Redirect("/swagger");
    }

    return Results.Ok(new
    {
        name = "Firmeza API",
        status = "Online",
        documentation = "/swagger",
        scalar = "/scalar/v1",
        endpoints = new[]
        {
            "/api/Auth",
            "/api/Products",
            "/api/Categories",
            "/api/Suppliers",
            "/api/PaymentMethods",
            "/api/Clients",
            "/api/Sales",
            "/api/Dispatches",
            "/api/Vehicles",
            "/api/Rentals",
            "/api/InventoryMovements",
            "/api/Dashboard",
            "/api/Users"
        },
        timestamp = DateTime.UtcNow
    });
}).WithName("ApiIndex");

// Mapear controladores de la API
app.MapControllers();

// Iniciar el servidor web
app.Run();