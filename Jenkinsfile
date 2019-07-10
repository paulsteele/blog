def label = "blog-ci"

properties([
  buildDiscarder(
    logRotator(numToKeepStr: '30')
  ),
  disableConcurrentBuilds()
])

podTemplate(label: label, serviceAccount: 'deployments', containers: [
  containerTemplate(name: 'docker', image: 'docker', command: 'cat', ttyEnabled: true),
  containerTemplate(name: 'kubectl', image: 'lachlanevenson/k8s-kubectl:v1.15.0', command: 'cat', ttyEnabled: true),
  containerTemplate(name: 'envsubst', image: 'loicmahieu/alpine-envsubst', command: 'cat', ttyEnabled: true)
],
volumes: [
  hostPathVolume(mountPath: '/var/run/docker.sock', hostPath: '/var/run/docker.sock')
]) {
  node(label) {
    def myRepo = checkout scm
    def gitCommit = myRepo.GIT_COMMIT
    def gitBranch = myRepo.GIT_BRANCH
    def shortGitCommit = "${gitCommit[0..10]}"
    def previousGitCommit = sh(script: "git rev-parse ${gitCommit}~", returnStdout: true)
    def tag = new Date().format('yyyy-dd-MM') + '-' + env.BUILD_NUMBER

    stage('Build') {
      container('docker') {
        sh "docker build . -t registry.paul-steele.com/blog:$tag"
      }
    }

    if (gitBranch == "master") {
      stage('Push to Registry') {
        container('docker') {
          withDockerRegistry([credentialsId: 'docker-registry', url: "https://registry.paul-steele.com/"]) {
            sh "docker push registry.paul-steele.com/blog:$tag"
          }
        }
      }

      stage('Deploy') {
        container('envsubst') {
          withEnv(["BUILD_TAG=$tag"]) {
            sh "envsubst < k8s.template.yaml > k8s.yaml"
          }
        }

        container('kubectl') {
          sh "kubectl apply -f ./k8s.yaml"
        }
      }
    }
  }
}
