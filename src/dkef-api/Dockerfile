FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY *.csproj .
RUN dotnet restore -r linux-arm64

COPY . .
RUN dotnet publish -c release -o /app --no-restore -r linux-arm64

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "dkef-api.dll" ]
