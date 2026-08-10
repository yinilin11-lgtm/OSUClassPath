FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OSUClassPath/OSUClassPath.csproj OSUClassPath/
RUN dotnet restore OSUClassPath/OSUClassPath.csproj

COPY . .
RUN dotnet publish OSUClassPath/OSUClassPath.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

CMD ["sh", "-c", "dotnet OSUClassPath.dll --urls http://0.0.0.0:${PORT:-8080}"]
