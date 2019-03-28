FROM node:10-alpine

WORKDIR /app
COPY . .
RUN yarn

ENTRYPOINT ["node", "lib/index.js"]