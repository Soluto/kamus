FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env

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
FROM mcr.microsoft.com/dotnet/aspnet:3.1-focal AS release
ARG PROJECT_NAME=decrypt-api
ENV PROJECT_NAME_ENV=$PROJECT_NAME
RUN groupadd -g 1000 dotnet && \
    useradd -u 1000 dotnet -g dotnet --home /home/dotnet

USER dotnet

WORKDIR /home/dotnet/app
ENV ASPNETCORE_URLS=http://+:9999
COPY --from=build-env --chown=dotnet:dotnet /app/obj/Docker/publish .

ENTRYPOINT dotnet $PROJECT_NAME_ENV.dll