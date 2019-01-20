# State 1: Dependendices
FROM node:10-alpine AS dependencies
WORKDIR /kamus-cli
COPY package.json yarn.lock ./
RUN yarn --producation

# State 2: Realise
FROM node:10-alpine AS release
COPY --from=dependencies /kamus-cli /kamus-cli
COPY . /kamus-cli
WORKDIR /kamus-cli

ENTRYPOINT ["node", "index.js"]