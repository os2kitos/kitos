FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG APP=Presentation.Web
WORKDIR /src

COPY . .

RUN if [ "$APP" = "Presentation.Web" ]; then \
        dotnet restore "Presentation.Web/Presentation.Web.csproj" -p:ContainerBuild=true; \
    elif [ "$APP" = "PubSub.Application.Api" ]; then \
        dotnet restore "PubSub.Application.Api/PubSub.Application.Api.csproj"; \
    else \
        echo "Unsupported APP=$APP" && exit 1; \
    fi

RUN if [ "$APP" = "Presentation.Web" ]; then \
        dotnet publish "Presentation.Web/Presentation.Web.csproj" -c Release -o /app/out -p:ContainerBuild=true; \
    elif [ "$APP" = "PubSub.Application.Api" ]; then \
        dotnet publish "PubSub.Application.Api/PubSub.Application.Api.csproj" -c Release -o /app/out; \
    else \
        echo "Unsupported APP=$APP" && exit 1; \
    fi

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG APP=Presentation.Web
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 ca-certificates \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
ENV APP=${APP}
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet ${APP}.dll"]
