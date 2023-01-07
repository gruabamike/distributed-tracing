FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ./Infrastructure ./Infrastructure
COPY ./Services/Inventory.Api ./Services/Inventory.Api

WORKDIR /src/Services/Inventory.Api/

RUN dotnet restore --ignore-failed-sources \
  && dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "Inventory.Api.dll" ]