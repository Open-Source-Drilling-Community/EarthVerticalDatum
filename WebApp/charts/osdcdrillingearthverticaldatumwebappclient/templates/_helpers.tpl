{{- define "earthverticaldatumwebapp.name" -}}{{ .Chart.Name | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthverticaldatumwebapp.fullname" -}}{{ default (printf "%s-%s" .Release.Name .Chart.Name) .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}{{- end }}
{{- define "earthverticaldatumwebapp.labels" -}}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "earthverticaldatumwebapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}
{{- define "earthverticaldatumwebapp.selectorLabels" -}}
app.kubernetes.io/name: {{ include "earthverticaldatumwebapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
{{- define "earthverticaldatumwebapp.serviceAccountName" -}}{{ if .Values.serviceAccount.create }}{{ default (include "earthverticaldatumwebapp.fullname" .) .Values.serviceAccount.name }}{{ else }}{{ default "default" .Values.serviceAccount.name }}{{ end }}{{- end }}
