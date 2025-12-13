# ETAPA 1: BUILD (Compilación)
# Usamos la imagen SDK de .NET 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y restauramos dependencias
# Esto optimiza la caché de Docker
COPY ["TechGear.Api.csproj", "./"]
RUN dotnet restore "TechGear.Api.csproj"

# Copiamos el resto del código fuente
COPY . .
WORKDIR "/src/."

# Compilamos en modo Release
RUN dotnet build "TechGear.Api.csproj" -c Release -o /app/build

# Publicamos los archivos finales
FROM build AS publish
RUN dotnet publish "TechGear.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: RUNTIME (Ejecución Ligera)
# Usamos la imagen Runtime de ASP.NET Core 10.0 (más liviana)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechGear.Api.dll"]