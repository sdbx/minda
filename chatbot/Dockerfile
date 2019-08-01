FROM node:8

WORKDIR /usr/src/app

COPY package.json ./

COPY tslib tslib

RUN cd tslib && npm install

WORKDIR /usr/src/app

RUN npm install

# TODO build inside the container

COPY build build

WORKDIR /home

CMD [ "node", "/usr/src/app/build/main/index.js" ]