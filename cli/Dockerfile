FROM node:16.15.0-alpine

WORKDIR /app
COPY . .
RUN yarn

ENTRYPOINT ["node", "lib/index.js"]
