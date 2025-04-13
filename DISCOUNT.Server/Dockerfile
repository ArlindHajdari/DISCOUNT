FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ENV PROTOBUF_TOOLS_OS=linux
ENV PROTOBUF_TOOLS_CPU=x64
ENV REDIS_CONNECTION_STRING="redis:6379"

COPY DISCOUNT.Server/DISCOUNT.Server.csproj .
COPY DISCOUNT.Server/Protos .
COPY Directory.Packages.props ./

RUN dotnet restore
COPY DISCOUNT.Server/ .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DISCOUNT.Server.dll"]
