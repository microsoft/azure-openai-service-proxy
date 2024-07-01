# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build


WORKDIR /src

# copy csproj and restore as distinct layers
COPY Proxy.NET.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY . .
RUN dotnet publish --no-restore -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./Proxy.NET"]
