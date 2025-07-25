var builder = WebApplication.CreateBuilder(args);

// включили подробное логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// слушаем внешний порт, который задаёт Render
builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "5001"));

// … все ваши сервисы …

var app = builder.Build();

// минимальный ответ для корня
app.MapGet("/", () => "Сервер работает!");

// выводим явный маркер готовности
Console.WriteLine(">>>>> Приложение готово к запуску <<<<<");

app.Run();