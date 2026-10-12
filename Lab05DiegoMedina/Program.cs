using System.Text;
using System.Text.Json.Serialization;
using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Repositories;
using Lab05DiegoMedina.Services;
using Lab05DiegoMedina.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Los roles se manejan como texto: "Admin", "Profesor", "Estudiante".
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Base de datos compartida (MySQL)
var connectionString = builder.Configuration.GetConnectionString("ConnectionMYSQLDB")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionMYSQLDB'.");

builder.Services.AddDbContext<ColegioDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositorio genérico + Unit of Work
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Servicios de la aplicación (un servicio por modelo)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IMateriaService, MateriaService>();
builder.Services.AddScoped<IMatriculaService, MatriculaService>();
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Configuración JWT leída directo de appsettings (Issuer, Audience, Key, ExpirationMinutes)
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Issuer'.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Audience'.");
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Key'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtTokenService.UserNameClaim,
            RoleClaimType = JwtTokenService.RoleClaim
        };
    });

// Políticas de acceso definidas directamente (nombres en texto para los controladores)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", policy =>
        policy.RequireAuthenticatedUser().RequireRole(Rol.Admin.ToString()));

    options.AddPolicy("ProfesorOAdmin", policy =>
        policy.RequireAuthenticatedUser().RequireRole(Rol.Admin.ToString(), Rol.Profesor.ToString()));

    options.AddPolicy("EstudianteOAdmin", policy =>
        policy.RequireAuthenticatedUser().RequireRole(Rol.Admin.ToString(), Rol.Estudiante.ToString()));
});

// Swagger con el botón "Authorize" para pegar el token
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Colegio ",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí el token que devuelve POST /api/Auth/login."
    });

    options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento, null)] = new List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Colegio v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();