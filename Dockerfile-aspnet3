FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

ARG PROJECT_NAME=decrypt-api

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY  ./src/$PROJECT_NAME/$PROJECT_NAME.csproj ./$PROJECT_NAME/$PROJECT_NAME.csproj
COPY ./src/key-managment/key-managment.csproj ./key-managment/key-managment.csproj 
RUN dotnet restore $PROJECT_NAME/$PROJECT_NAME.csproj

# Copy everything else and build
COPY  ./src/$PROJECT_NAME ./$PROJECT_NAME
COPY  ./src/key-managment ./key-managment
RUN dotnet publish $PROJECT_NAME/$PROJECT_NAME.csproj -c Release -o ./obj/Docker/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS release
ARG PROJECT_NAME=decrypt-api
ENV PROJECT_NAME_ENV=$PROJECT_NAME
RUN addgroup dotnet && \
    adduser -D -G dotnet -h /home/dotnet dotnet && \
    apk --no-cache add ca-certificates wget && \
    wget -q -O /etc/apk/keys/sgerrand.rsa.pub https://alpine-pkgs.sgerrand.com/sgerrand.rsa.pub && \
    wget -q https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.30-r0/glibc-2.30-r0.apk && \
    apk add glibc-2.30-r0.apk && \
    apk del wget

USER dotnet

WORKDIR /home/dotnet/app
ENV ASPNETCORE_URLS=http://+:9999
COPY --from=build-env /app/obj/Docker/publish .

ENTRYPOINT dotnet $PROJECT_NAME_ENV.dll