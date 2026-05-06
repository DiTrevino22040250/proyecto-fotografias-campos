FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FotografiasCampos.Api/FotografiasCampos.Api.csproj", "FotografiasCampos.Api/"]
COPY ["FotografiasCampos.Api.Application/FotografiasCampos.Api.Application.csproj", "FotografiasCampos.Api.Application/"]
COPY ["FotografiasCampos.Api.Domain/FotografiasCampos.Api.Domain.csproj", "FotografiasCampos.Api.Domain/"]
COPY ["FotografiasCampos.Api.Infrastructure/FotografiasCampos.Api.Infrastructure.csproj", "FotografiasCampos.Api.Infrastructure/"]
RUN dotnet restore "FotografiasCampos.Api/FotografiasCampos.Api.csproj"
COPY . .
WORKDIR "/src/FotografiasCampos.Api"
RUN dotnet build "FotografiasCampos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FotografiasCampos.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FotografiasCampos.Api.dll"]
