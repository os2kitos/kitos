FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR app

COPY ["PubSub.Application/PubSub.Application.csproj", "PubSub.Application/"]
COPY ["PubSub.Core/PubSub.Core.csproj", "PubSub.Core/"]

RUN dotnet restore "PubSub.Application/PubSub.Application.csproj"

COPY . .

RUN dotnet publish "PubSub.Application/PubSub.Application.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 443

ENTRYPOINT ["dotnet", "PubSub.Application.dll"]
