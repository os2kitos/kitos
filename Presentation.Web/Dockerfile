FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["Presentation.Web/Presentation.Web.csproj", "Presentation.Web/"]
COPY ["Core.Abstractions/Core.Abstractions.csproj", "Core.Abstractions/"]
COPY ["Core.ApplicationServices/Core.ApplicationServices.csproj", "Core.ApplicationServices/"]
COPY ["Core.BackgroundJobs/Core.BackgroundJobs.csproj", "Core.BackgroundJobs/"]
COPY ["Core.DomainModel/Core.DomainModel.csproj", "Core.DomainModel/"]
COPY ["Core.DomainServices/Core.DomainServices.csproj", "Core.DomainServices/"]
COPY ["Infrastructure.DataAccess/Infrastructure.DataAccess.csproj", "Infrastructure.DataAccess/"]
COPY ["Infrastructure.Ninject/Infrastructure.Ninject.csproj", "Infrastructure.Ninject/"]
COPY ["Infrastructure.OpenXML/Infrastructure.OpenXML.csproj", "Infrastructure.OpenXML/"]
COPY ["Infrastructure.Services/Infrastructure.Services.csproj", "Infrastructure.Services/"]
COPY ["Infrastructure.STS.Common/Infrastructure.STS.Common.csproj", "Infrastructure.STS.Common/"]
COPY ["Infrastructure.STS.Company/Infrastructure.STS.Company.csproj", "Infrastructure.STS.Company/"]
COPY ["Infrastructure.STS.Organization/Infrastructure.STS.Organization.csproj", "Infrastructure.STS.Organization/"]
COPY ["Infrastructure.STS.OrganizationSystem/Infrastructure.STS.OrganizationSystem.csproj", "Infrastructure.STS.OrganizationSystem/"]

RUN dotnet restore "Presentation.Web/Presentation.Web.csproj" -p:ContainerBuild=true

COPY . .

RUN dotnet publish "Presentation.Web/Presentation.Web.csproj" -c Release -o out -p:ContainerBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 ca-certificates \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Presentation.Web.dll"]
