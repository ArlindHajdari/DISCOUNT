FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 6000

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ENV PROTOBUF_TOOLS_OS=linux
ENV PROTOBUF_TOOLS_CPU=x64
ENV GRPC_SERVER_ADDRESS="http://discountserver:5001"

COPY DISCOUNT.Client/DISCOUNT.Client.csproj .
COPY DISCOUNT.Client/Protos .
COPY Directory.Packages.props ./

RUN dotnet restore
COPY DISCOUNT.Client/ .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DISCOUNT.Client.dll"]
