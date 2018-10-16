FROM mhart/alpine-node:10.12.0

# Create app directory
WORKDIR /usr/src/app

# Install app dependencies
# A wildcard is used to ensure both package.json AND package-lock.json are copied
# where available (npm@5+)
COPY package*.json yarn.lock ./

RUN yarn --prod

# Bundle app source
COPY . .

CMD [ "node", "index.js" ]