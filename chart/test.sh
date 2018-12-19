set -e

echo "Without airbag"
helm template . -f tests/values-without-airbag.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/without-airbag --verbose

echo "With airbag"
helm template . -f tests/values-with-airbag.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-airbag --verbose

echo "With annotations"
helm template . -f tests/values-with-annotations.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-annotations --verbose

echo "With command and args"
helm template . -f tests/values-with-command-and-args.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-command-and-args --verbose

echo "With custom probes"
helm template . -f tests/values-with-custom-probes.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-custom-probes --verbose

echo "With role"
helm template . -f tests/values-with-role.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-role --verbose

echo "with ingress path"
helm template . -f tests/values-with-ingress-path.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-ingress-path --verbose

echo "with custom tls cert"
helm template . -f tests/values-with-custom-tls-cert.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-custom-tls-cert --verbose

echo "with encrypted secrets"
helm template . -f tests/values-with-encrypted-secrets.yaml > template.yaml
cat template.yaml | kubetest -t tests/common --verbose
cat template.yaml | kubetest -t tests/with-encrypted-secrets --verbose

echo "with long nameOverride"
helm template . -f tests/values-with-long-nameOverride.yaml > template.yaml
cat template.yaml | kubetest -t tests/with-long-nameOverride --verbose

echo "with virtual service"
helm template . -f tests/values-with-virtual-service.yaml > template.yaml
cat template.yaml | kubetest -t tests/with-virtual-service --verbose
