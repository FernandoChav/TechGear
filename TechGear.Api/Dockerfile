# ETAPA 1: BUILD
# Usamos la imagen del SDK de .NET 10 (La más nueva en 2025)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el csproj y restauramos
COPY ["TechGear.Api.csproj", "./"]
RUN dotnet restore "TechGear.Api.csproj"

# Copiamos todo lo demas
COPY . .
WORKDIR "/src/."
RUN dotnet build "TechGear.Api.csproj" -c Release -o /app/build

# Publicamos
FROM build AS publish
RUN dotnet publish "TechGear.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: RUNTIME
# Usamos el Runtime de .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechGear.Api.dll"]