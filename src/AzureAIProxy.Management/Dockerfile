# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH

WORKDIR /src

# copy csproj and restore as distinct layers
COPY AzureAIProxy.Management.csproj .
RUN dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./AzureAIProxy.Management"]
