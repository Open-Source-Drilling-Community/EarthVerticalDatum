{{- define "earthverticaldatumservice.name" -}}{{ .Chart.Name | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthverticaldatumservice.fullname" -}}{{ default (printf "%s-%s" .Release.Name .Chart.Name) .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthverticaldatumservice.labels" -}}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "earthverticaldatumservice.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}
{{- define "earthverticaldatumservice.selectorLabels" -}}
app.kubernetes.io/name: {{ include "earthverticaldatumservice.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
{{- define "earthverticaldatumservice.serviceAccountName" -}}{{ if .Values.serviceAccount.create }}{{ default (include "earthverticaldatumservice.fullname" .) .Values.serviceAccount.name }}{{ else }}{{ default "default" .Values.serviceAccount.name }}{{ end }}{{- end }}
