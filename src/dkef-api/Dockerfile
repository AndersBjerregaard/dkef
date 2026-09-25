FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
# Alpine runtime images do not include IANA timezone data by default.
# The API resolves Europe/Copenhagen in Utilities/CopenhagenDateTime.cs, so tzdata is required in production.
RUN apk add --no-cache tzdata
COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "dkef-api.dll" ]
