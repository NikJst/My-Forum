FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyForum.csproj", "./"]
RUN dotnet restore "./MyForum.csproj"

COPY . .
RUN dotnet publish "MyForum.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyForum.dll"]