FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR app

COPY ["PubSub.Application.Api/PubSub.Application.Api.csproj", "PubSub.Application.Api/"]
COPY ["PubSub.Application.Services/PubSub.Application.Services.csproj", "PubSub.Application.Services/"]
COPY ["PubSub.Core.Abstractions/PubSub.Core.Abstractions.csproj", "PubSub.Core.Abstractions/"]
COPY ["PubSub.Core.Interfaces/PubSub.Core.Interfaces.csproj", "PubSub.Core.Interfaces/"]
COPY ["PubSub.Core.DomainModel/PubSub.Core.DomainModel.csproj", "PubSub.Core.DomainModel/"]
COPY ["PubSub.Infrastructure.DataAccess/PubSub.Infrastructure.DataAccess.csproj", "PubSub.Infrastructure.DataAccess/"]
COPY ["PubSub.Infrastructure.MessageQueue/PubSub.Infrastructure.MessageQueue.csproj", "PubSub.Infrastructure.MessageQueue/"]

RUN dotnet restore "PubSub.Application.Api/PubSub.Application.Api.csproj"

COPY . .

RUN dotnet publish "PubSub.Application.Api/PubSub.Application.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 443

ENTRYPOINT ["dotnet", "PubSub.Application.Api.dll"]