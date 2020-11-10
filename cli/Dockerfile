FROM node:14.8-alpine

WORKDIR /app
COPY . .
RUN yarn

ENTRYPOINT ["node", "lib/index.js"]
