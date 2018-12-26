workflow "Build" {
  on = "push"
  resolves = ["GitHub Action for Docker"]
}

action "Build Decryptor API" {
  uses = "actions/docker/cli@76ff57a"
  runs = "docker build ."
}

action "Build Encryptor API" {
  uses = "actions/docker/cli@76ff57a"
  runs = "docker build . --build-arg PROJECT_NAME=encrypt-api"
}