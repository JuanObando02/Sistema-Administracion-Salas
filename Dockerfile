# Etapa de compilación (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyectos para restaurar dependencias primero (aprovecha la caché de Docker)
COPY ["Web/MvcSample/MvcSample.csproj", "Web/MvcSample/"]
COPY ["Services/Services/Services.csproj", "Services/Services/"]
COPY ["Domain/Domain/Domain.csproj", "Domain/Domain/"]
COPY ["Infrastructure/Infrastructure/Infrastructure.csproj", "Infrastructure/Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Web/MvcSample/MvcSample.csproj"

# Copiar el resto del código fuente del proyecto
COPY . .
WORKDIR "/src/Web/MvcSample"

# Compilar en modo Release
RUN dotnet build "MvcSample.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "MvcSample.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Ejecución en imagen optimizada)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Exponer el puerto por defecto de .NET 8/9
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MvcSample.dll"]
