FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Turnero.csproj", "./"]
COPY ["Turnero.DAL/Turnero.DAL.csproj", "Turnero.DAL/"]
COPY ["Turnero.SL/Turnero.SL.csproj", "Turnero.SL/"]
COPY ["Turnero.Utilities/Turnero.Utilities.csproj", "Turnero.Utilities/"]
RUN dotnet restore "Turnero.csproj"

COPY . .
RUN dotnet publish "Turnero.csproj" -c Release -r linux-musl-x64 --self-contained true -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app

# curl en Alpine via apk pesa ~2 MB (vs ~40 MB en Ubuntu), indispensable para healthchecks
# nc (netcat) se usa en wait-for-it.sh para sondear conexiones TCP
RUN apk add --no-cache curl netcat-openbsd

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# wait-for-it.sh: espera a que servicios (Postgres) estén listos antes de arrancar
# docker-entrypoint.sh: entrypoint que procesa WAIT_FOR_SERVICES y ejecuta la app
COPY wait-for-it.sh docker-entrypoint.sh /app/
RUN chmod +x /app/wait-for-it.sh /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]
