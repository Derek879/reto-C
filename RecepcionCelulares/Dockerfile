FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/RecepcionCelulares.Api/RecepcionCelulares.Api.csproj src/RecepcionCelulares.Api/
RUN dotnet restore src/RecepcionCelulares.Api/RecepcionCelulares.Api.csproj
COPY src/ src/
RUN dotnet publish src/RecepcionCelulares.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RecepcionCelulares.Api.dll"]
