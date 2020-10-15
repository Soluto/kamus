FROM node:14-alpine

WORKDIR /app
COPY . .
RUN yarn

ENTRYPOINT ["node", "lib/index.js"]
