FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as source

WORKDIR /app

RUN  apt-get update \
  && apt-get install -y jq \
  && rm -rf /var/lib/apt/lists/*
RUN wget https://raw.githubusercontent.com/vishnubob/wait-for-it/8ed92e8cab83cfed76ff012ed4a36cef74b28096/wait-for-it.sh
RUN chmod +x wait-for-it.sh

COPY blackbox.csproj .
RUN dotnet restore blackbox.csproj
COPY . .
RUN dotnet build -c Release

ENTRYPOINT ["./run_test.sh"]