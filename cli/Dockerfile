FROM node:10-alpine
ARG CLI_VERSION=0.0.6
RUN yarn global add @soluto-asurion/kamus-cli@${CLI_VERSION}

ENTRYPOINT ["kamus-cli"]