FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY * ./
ENTRYPOINT ["dotnet", "_MicroserviceTemplate_.WebApi.dll"]