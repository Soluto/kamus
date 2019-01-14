workflow "Build" {
  on = "push"
  resolves = ["Build Decryptor API", "Build Encryptor API"]
}

action "Build Decryptor API" {
  uses = "actions/docker/cli@76ff57a"
  runs = "docker-compose -v"
}

action "Build Encryptor API" {
  uses = "actions/docker/cli@76ff57a"
  runs = "docker build . --build-arg PROJECT_NAME=encrypt-api"
}
