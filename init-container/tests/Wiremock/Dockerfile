FROM openjdk:8-stretch

WORKDIR /wiremock

RUN wget http://repo1.maven.org/maven2/com/github/tomakehurst/wiremock-standalone/2.17.0/wiremock-standalone-2.17.0.jar

COPY . .

CMD ["java", "-jar", "wiremock-standalone-2.17.0.jar"]