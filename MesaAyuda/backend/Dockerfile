FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/MesaAyuda.Domain/*.csproj src/MesaAyuda.Domain/
COPY src/MesaAyuda.Application/*.csproj src/MesaAyuda.Application/
COPY src/MesaAyuda.Infrastructure/*.csproj src/MesaAyuda.Infrastructure/
COPY src/MesaAyuda.Api/*.csproj src/MesaAyuda.Api/
RUN dotnet restore src/MesaAyuda.Api/MesaAyuda.Api.csproj
COPY src/ src/
RUN dotnet publish src/MesaAyuda.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MesaAyuda.Api.dll"]
