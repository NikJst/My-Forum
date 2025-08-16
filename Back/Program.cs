using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyForum.Hubs;
using MyForum.Models;

var builder = WebApplication.CreateBuilder(args);


Console.WriteLine("JWT Key из конфигурации: " + builder.Configuration["Jwt:Key"]);//проверка

builder.Services.AddDbContext<ForumContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем поддержку CORS для локального хоста
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5500", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // <- добавляем
    });
});

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


// включили подробное логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// слушаем внешний порт
builder.WebHost.UseUrls("http://+:8000");

// … все ваши сервисы …
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_123!"; // или в appsettings
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyForum";

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

//==========>
var app = builder.Build();
app.UseSwagger();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Настройка CORS
app.UseCors("AllowLocalhost5500");
app.MapControllers();
app.MapHub<ForumHub>("/forumHub");// Регистрация хаба SignalR



if (app.Environment.IsDevelopment() || true) // 👈 можно убрать `|| true` позже
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Forum API v1");
        options.RoutePrefix = "swagger"; // чтобы Swagger был по адресу /swagger
    });
}

app.MapControllers();

// минимальный ответ для корня


// выводим явный маркер готовности
Console.WriteLine(">>>>> Приложение готово к запуску <<<<<");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ForumContext>();
    dbContext.Database.Migrate();
}
app.Run();