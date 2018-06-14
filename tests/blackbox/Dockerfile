FROM microsoft/aspnetcore-build:2.0.5-2.1.4 as source

COPY . .

RUN  apt-get update \
  && apt-get install -y jq \
  && rm -rf /var/lib/apt/lists/*
RUN wget https://raw.githubusercontent.com/vishnubob/wait-for-it/8ed92e8cab83cfed76ff012ed4a36cef74b28096/wait-for-it.sh
RUN chmod +x wait-for-it.sh

RUN dotnet restore ./Hamuste.sln
RUN dotnet build Hamuste.sln -c Release

RUN chmod +x ./scripts/run_test.sh
ENTRYPOINT ["./scripts/run_test.sh"]