FROM public.ecr.aws/amazonlinux/amazonlinux:latest
RUN yum install dotnet-sdk-6.0 -y
#RUN pip install opentelemetry-distro opentelemetry-exporter-otlp
#FROM mcr.microsoft.com/dotnet/sdk:6.0
#RUN opentelemetry-bootstrap --action=install
#ENV OTEL_PYTHON_DISABLED_INSTRUMENTATIONS=urllib3
#ENV OTEL_METRICS_EXPORTER=none              
#ENV OTEL_RESOURCE_ATTRIBUTES='service.name=dass_hello_app'
#ENV OTEL_PROPAGATORS=xray
#ENV OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=0.0.0.0:4317
COPY publish /app
EXPOSE 5050
#CMD opentelemetry-instrument dotnet /app/MyApp.dll --urls=http://0.0.0.0:5050
CMD dotnet /app/MyApp.dll --urls=http://0.0.0.0:5050

