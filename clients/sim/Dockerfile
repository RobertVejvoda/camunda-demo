FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["clients/sim/sim.csproj", "clients/sim/"]
COPY ["shared/Common/Common.csproj", "shared/Common/"]
COPY ["blocks/Camunda/Camunda.csproj", "blocks/camunda/"]
RUN dotnet restore "clients/sim/sim.csproj"
COPY . .
WORKDIR "/src/clients/sim"
RUN dotnet build "sim.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "sim.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sim.dll"]
