# IberaReclamos API — imagen para Railway (o cualquier hosting con Docker).
# 1) Compila con el SDK de .NET 10   2) Corre solo con el runtime de ASP.NET (imagen más liviana)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero los .csproj: si no cambian, Docker reutiliza la capa del restore y el build es más rápido
COPY IberaReclamos.API/IberaReclamos.API.csproj IberaReclamos.API/
COPY IberaReclamos.Application/IberaReclamos.Application.csproj IberaReclamos.Application/
COPY IberaReclamos.Domain/IberaReclamos.Domain.csproj IberaReclamos.Domain/
COPY IberaReclamos.Infrastructure/IberaReclamos.Infrastructure.csproj IberaReclamos.Infrastructure/
RUN dotnet restore IberaReclamos.API/IberaReclamos.API.csproj

COPY . .
RUN dotnet publish IberaReclamos.API/IberaReclamos.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
# Railway pasa el puerto en PORT (Program.cs lo lee); 8080 es el valor por defecto fuera de Railway
EXPOSE 8080
ENTRYPOINT ["dotnet", "IberaReclamos.API.dll"]
