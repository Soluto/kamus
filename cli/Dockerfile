FROM node:16.5.0-alpine

WORKDIR /app
COPY . .
RUN yarn

ENTRYPOINT ["node", "lib/index.js"]
