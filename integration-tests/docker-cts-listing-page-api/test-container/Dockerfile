FROM openjdk:11

ADD ./bin /usr/src/app/bin
ADD ./data/source /usr/src/app/data/source
WORKDIR /usr/src/app
CMD [ "/usr/src/app/bin/run-integration-tests.sh" ]