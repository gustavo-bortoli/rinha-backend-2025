FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-preview-alpine AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine-aot AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./Payments.csproj", "./"]
RUN dotnet restore "./Payments.csproj"
COPY . ./
RUN dotnet build "./Payments.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Payments.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Payments"]