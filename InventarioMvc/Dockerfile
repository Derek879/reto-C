FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/InventarioMvc.Web/InventarioMvc.Web.csproj src/InventarioMvc.Web/
RUN dotnet restore src/InventarioMvc.Web/InventarioMvc.Web.csproj
COPY src/ src/
COPY database/ database/
RUN dotnet publish src/InventarioMvc.Web -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventarioMvc.Web.dll"]
