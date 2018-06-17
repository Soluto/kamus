FROM microsoft/dotnet:2.0-sdk-stretch AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/Hamuste.csproj ./
RUN dotnet restore && \
    dotnet add package ILLink.Tasks -v 0.1.4-preview-981901 -s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json

# Copy everything else and build
COPY ./src ./
RUN dotnet publish -c Release -o out -r linux-x64

# Build runtime image
FROM microsoft/dotnet:2.0-runtime-deps
RUN useradd -d /home/dotnet -ms /bin/bash dotnet
USER dotnet
WORKDIR /home/ubuntu/app
ENV ASPNETCORE_URLS=http://+:9999
COPY --from=build-env /app/out ./
ENTRYPOINT ["./Hamuste"]