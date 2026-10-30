FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Turnero.Domain/Turnero.Domain.csproj", "src/Turnero.Domain/"]
COPY ["src/Turnero.Application/Turnero.Application.csproj", "src/Turnero.Application/"]
COPY ["src/Turnero.Infrastructure/Turnero.Infrastructure.csproj", "src/Turnero.Infrastructure/"]
COPY ["src/Turnero.Web/Turnero.Web.csproj", "src/Turnero.Web/"]
RUN dotnet restore "src/Turnero.Web/Turnero.Web.csproj"

COPY . .
RUN dotnet publish "src/Turnero.Web/Turnero.Web.csproj" -c Release -r linux-musl-x64 --self-contained true -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app

# curl en Alpine via apk pesa ~2 MB (vs ~40 MB en Ubuntu), indispensable para healthchecks
# nc (netcat) se usa en wait-for-it.sh para sondear conexiones TCP
RUN apk add --no-cache curl netcat-openbsd

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Turnero.Web.dll"]
