using ForumBackend;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ForumBackend.Hubs;
using ForumBackend.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "5000"));


// Подключение базы данных
builder.Services.AddDbContext<ForumContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Подключение контроллеров и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Forum API", Version = "v1" });

    // 👇 Добавляем поддержку JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
//====
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_123!"; // или в appsettings
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ForumBackend";

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
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
//
builder.Services.AddSignalR();

// Настройка CORS
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.Listen(System.Net.IPAddress.Loopback, 5000); // фиксированный порт 5000
// }); 
// builder.WebHost.UseKestrel(options => options.ListenAnyIP(5000)); // или любой IP
var app = builder.Build();

// // Middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Forum API V1");
    options.RoutePrefix = "swagger";
});
app.UseStaticFiles();
app.UseDefaultFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ForumHub>("/forumHub");// Регистрация хаба SignalR

//====
app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddresses = app.Urls;
    foreach (var addr in serverAddresses)
    {
        Console.WriteLine($"Сервер запущен по адресу: {addr}");
    }
});


// // Перенаправление корневого URL на Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


app.Run();