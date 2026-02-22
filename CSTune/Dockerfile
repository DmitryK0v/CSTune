FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо тільки файл .csproj, щоб cache працював
COPY *.csproj ./

# Відновлення залежностей
RUN dotnet restore "CSTune.csproj"

# Копіюємо всі файли проєкту
COPY . .

# Компіляція
RUN dotnet publish -c Release -o /app/publish

# Production образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Копіюємо результат публікації
COPY --from=build /app/publish .

# Порт, який слухає ASP.NET
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "CSTune.dll"]