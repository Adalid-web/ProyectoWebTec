FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ProyectoTecWeb.csproj", "./"]
RUN dotnet restore "ProyectoTecWeb.csproj"

COPY . .
RUN dotnet build "ProyectoTecWeb.csproj" -c Release -o /app/build --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/build .
COPY --from=build /src/wwwroot ./wwwroot

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "ProyectoTecWeb.dll"]
