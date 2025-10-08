# -----------------------------
# 1️⃣ Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Solution dosyasını kopyala ve restore et
COPY *.sln ./
COPY PromptValueEstimator.Application/*.csproj PromptValueEstimator.Application/
COPY PromptValueEstimator.Infrastructure/*.csproj PromptValueEstimator.Infrastructure/
COPY PromptValueEstimator.Api/*.csproj PromptValueEstimator.Api/

RUN dotnet restore PromptValueEstimator.Api/PromptValueEstimator.Api.csproj

# Tüm kaynak kodu kopyala ve publish et
COPY . .
RUN dotnet publish PromptValueEstimator.Api/PromptValueEstimator.Api.csproj -c Release -o /app/out

# -----------------------------
# 2️⃣ Runtime Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PromptValueEstimator.Api.dll"]
