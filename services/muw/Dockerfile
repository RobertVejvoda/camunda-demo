FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/muw/muw.csproj", "services/muw/"]
COPY ["shared/Common/Common.csproj", "shared/Common/"]
COPY ["blocks/Camunda/Camunda.csproj", "blocks/camunda/"]
RUN dotnet restore "services/muw/muw.csproj"
COPY . .
WORKDIR "/src/services/muw"
RUN dotnet build "muw.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "muw.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "muw.dll"]
