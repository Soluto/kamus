workflow "Build" {
  on = "push"
  resolves = ["GitHub Action for Docker"]
}

action "GitHub Action for Docker" {
  uses = "actions/docker/cli@76ff57a"
  runs = "docker build ."
}
