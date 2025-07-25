# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем проект в контейнер
COPY . ./

# Публикуем в папку out
RUN dotnet publish -c Release -o out

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Указываем порт
EXPOSE 8080

# Указываем переменную окружения для URL
ENV ASPNETCORE_URLS=http://+:8080

# Указываем команду запуска
ENTRYPOINT ["dotnet", "ForumBackend.dll"]