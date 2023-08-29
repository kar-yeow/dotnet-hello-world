FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY publish /app
ENTRYPOINT ["dotnet", "/app/MyApp.dll", "--urls=http://0.0.0.0:5050"]

