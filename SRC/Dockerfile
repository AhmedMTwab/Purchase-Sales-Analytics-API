# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all .csproj files first (layer cache for restore)
COPY SRC.sln .
COPY Purchase\&Sales_API/Purchase\&Sales_API.csproj          Purchase\&Sales_API/
COPY Purchase\&Sales_Core/Purchase\&Sales_Core.csproj        Purchase\&Sales_Core/
COPY Purchase\&Sales_Domain/Purchase\&Sales_Domain.csproj    Purchase\&Sales_Domain/
COPY Purchase\&Sales_Infrastructure/Purchase\&Sales_Infrastructure.csproj Purchase\&Sales_Infrastructure/

RUN dotnet restore SRC.sln

# Copy the rest of the source
COPY . .

# Publish the API project
RUN dotnet publish Purchase\&Sales_API/Purchase\&Sales_API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Purchase&Sales_API.dll"]
