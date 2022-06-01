FROM node:14.18.3-alpine3.14

RUN mkdir /home/node/app
# Create app directory
WORKDIR /home/node/app

# Install app dependencies
# A wildcard is used to ensure both package.json AND package-lock.json are copied
# where available (npm@5+)
COPY package*.json yarn.lock ./

RUN yarn --prod 

# Bundle app source
COPY . .

USER node

ENTRYPOINT [ "node", "index.js" ]