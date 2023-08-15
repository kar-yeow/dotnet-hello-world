FROM artifactory.ctz.atocnet.gov.au/remote-microsoft-docker/dotnet/sdk:7.0
COPY MyApp/publish /app
ENTRYPOINT ["dotnet", "/app/MyApp.dll"]

