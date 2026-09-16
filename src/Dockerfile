# Salesforce CLI container for repeatable metadata validation.
FROM salesforce/cli:latest
WORKDIR /workspace
COPY . .
# TODO: Inject authentication securely in CI; never bake credentials into the image.
CMD ["sf", "project", "deploy", "preview", "--source-dir", "force-app"]
