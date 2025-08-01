# ---------- ЭТАП 1: СБОРКА ПРОЕКТА ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build   
WORKDIR /app                                     
# Копируем только файлы проекта для восстановления зависимостей
COPY ForumBackend.csproj ./
# Восстанавливаем зависимости внутри контейнера
RUN dotnet restore
# Копируем все остальные файлы проекта
COPY . ./
# Публикуем проект в режиме Release
# Эта команда создаст скомпилированную версию в папке /app/out
RUN dotnet publish ForumBackend.csproj -c Release -o /app/out
# ---------- ЭТАП 2: ЗАПУСК СКОМПИЛИРОВАННОГО ПРИЛОЖЕНИЯ ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0         
WORKDIR /app                                     
# Копируем результат сборки из предыдущего этапа
COPY --from=build /app/out/ .
# Открываем порт (необязательно, но полезно для локального проброса)
EXPOSE 8080
# Указываем URL, по которому будет слушать приложение
ENV ASPNETCORE_URLS=http://+:8000
# Основная команда запуска
ENTRYPOINT ["dotnet", "ForumBackend.dll"]