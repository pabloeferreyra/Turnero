FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Turnero.csproj", "./"]
COPY ["Turnero.DAL/Turnero.DAL.csproj", "Turnero.DAL/"]
COPY ["Turnero.SL/Turnero.SL.csproj", "Turnero.SL/"]
COPY ["Turnero.Utilities/Turnero.Utilities.csproj", "Turnero.Utilities/"]
RUN dotnet restore "Turnero.csproj"

COPY . .
RUN dotnet publish "Turnero.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Turnero.dll"]
