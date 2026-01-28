FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out --self-contained false


FROM mcr.microsoft.com/dotnet/runtime:10.0-slim
WORKDIR /app

COPY --from=builder /app/out ./


ENTRYPOINT ["dotnet", "YourAppName.dll"]
