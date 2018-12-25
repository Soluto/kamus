{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "kamus.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "appsettings.secret.json" }}
{{ printf "{\n\t\"ActiveDirectory\": { " }}
{{ if .Values.activeDirectory}}
{{ printf "\t\t\"ClientSecret\": \"%s\" " .Values.activeDirectory.clientSecret }}
{{- end -}}
{{ if .Values.keyManagment.AES}}
{{ printf "\"KeyManagement\": { \n\t\t\"AES\": { \"Key\": \"%s\" } }" .Values.keyManagment.AES.key }}
{{- end -}}
{{ printf "} \n}"}}
{{- end }}