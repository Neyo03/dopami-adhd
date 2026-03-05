FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "LifeManager.csproj"
RUN dotnet publish "LifeManager.csproj" -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --configuration Release --self-contained -r linux-x64 -o /app/publish/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

RUN chmod +x ./efbundle

EXPOSE 8080
ENTRYPOINT ["dotnet", "LifeManager.dll"]