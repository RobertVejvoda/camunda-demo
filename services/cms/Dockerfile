FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/cms/cms.csproj", "services/cms/"]
COPY ["shared/Common/Common.csproj", "shared/Common/"]
RUN dotnet restore "services/cms/cms.csproj"
COPY . .
WORKDIR "/src/services/cms"
RUN dotnet build "cms.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cms.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cms.dll"]
