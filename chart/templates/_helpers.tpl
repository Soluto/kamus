{{/*
Expand the name of the chart.
*/}}
{{- define "kamus.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "appsettings.secrets.json" }}
{{ printf "{\n\t\"ActiveDirectory\": { " }}
{{ if .Values.keyManagment.azureKeyVault}}
{{ printf "\t\t\"ClientSecret\": \"%s\" " .Values.keyManagment.azureKeyVault.clientSecret }}
{{- end -}}
{{ if .Values.keyManagment.AES}}
{{ printf "\"KeyManagement\": { \n\t\t\"AES\": { \"Key\": \"%s\" } }" .Values.keyManagment.AES.key }}
{{- end -}}
{{ printf "} \n}"}}
{{- end }}

{{- define "common.configurations" -}}
KeyManagement__Provider: {{ .Values.keyManagment.provider }}
{{ if .Values.keyManagment.azureKeyVault }}
KeyManagement__KeyVault__Name: {{ .Values.keyManagment.azureKeyVault.keyVaultName }}
KeyManagement__KeyVault__KeyType: {{ default "RSA-HSM" .Values.keyManagment.azureKeyVault.keyType }}
KeyManagement__KeyVault__KeyLength: {{ default "2048" .Values.keyManagment.azureKeyVault.keySize | quote }}
KeyManagement__KeyVault__MaximumDataLength: {{ default "214" .Values.keyManagment.azureKeyVault.maximumDataLength | quote }}
ActiveDirectory__ClientId: {{ .Values.keyManagment.azureKeyVault.clientId }}
{{ end }}
{{- end -}}}}
