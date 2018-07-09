FROM microsoft/aspnetcore-build:2.0.5-2.1.4 AS build-env

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY  ./src/Hamuste.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY  ./src ./
RUN dotnet publish -c Release -o ./obj/Docker/publish

# Build runtime image
FROM microsoft/aspnetcore:2.0.8 as release
RUN groupadd -r dotnet && useradd --no-log-init -r -g dotnet -d /home/dotnet -ms /bin/bash dotnet
USER dotnet
WORKDIR /home/dotnet/app
ENV ASPNETCORE_URLS=http://+:9999
COPY --from=build-env /app/obj/Docker/publish .
ENTRYPOINT ["dotnet", "Hamuste.dll"]