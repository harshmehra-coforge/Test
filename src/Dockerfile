# Salesforce CLI development container; no patient data belongs in the image.
FROM salesforce/cli:latest
WORKDIR /workspace
COPY . .
CMD ["sf", "project", "deploy", "start", "--help"]
