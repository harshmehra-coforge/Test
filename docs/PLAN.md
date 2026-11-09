# PLAN.md

## Stack & Rationale

- **Language**: Python 3.11
- **Frameworks/Libraries**:
  - PyTorch (for model fine-tuning)
  - HuggingFace Transformers & PEFT (for LoRA/QLoRA)
  - FastAPI (for optional REST API)
  - Docker (containerization)
- **Deployment**: Azure Foundry compute (container-based deployment)
- **Config**: 12-factor, via environment variables
- **Testing**: pytest

**Rationale**: Python is the de facto standard for ML/AI and model fine-tuning. LoRA/QLoRA are best supported in HuggingFace/PEFT. Docker ensures reproducibility and Azure Foundry supports containerized workloads.

## Module/Service Breakdown

- **src/**
  - `main.py`: Entrypoint CLI/REST API
  - `trainer/`
    - `data.py`: Data loading/prep utilities
    - `lora.py`: LoRA fine-tuning logic
    - `qlora.py`: QLoRA fine-tuning logic
    - `utils.py`: Shared utilities
    - `config.py`: Training/config management
  - `api/`
    - `routes.py`: (Optional) REST endpoints for triggering jobs
  - `docker/`
    - `build.py`: Docker image build script (if needed)
  - `deployment/`
    - `azure_foundry.py`: Azure deployment logic/stubs
- **tests/**: Mirrors src/ structure
- **.env.example**: Example environment variables
- **requirements.txt**: Python dependencies
- **Dockerfile**: Container build
- **README.md**: Setup, usage, deployment docs

## Target Folder Structure

```
project-root/
  src/
    main.py                  # Entrypoint CLI/REST API
    trainer/
      data.py                # Data loading/prep
      lora.py                # LoRA fine-tuning logic
      qlora.py               # QLoRA fine-tuning logic
      utils.py               # Shared utilities
      config.py              # Training/config management
    api/
      routes.py              # REST endpoints (optional)
    docker/
      build.py               # Docker image build script (optional)
    deployment/
      azure_foundry.py       # Azure deployment logic/stubs
  tests/
    test_main.py
    trainer/
      test_data.py
      test_lora.py
      test_qlora.py
      test_utils.py
      test_config.py
    api/
      test_routes.py
    docker/
      test_build.py
    deployment/
      test_azure_foundry.py
  requirements.txt
  Dockerfile
  .env.example
  README.md
```

## Build Order & Milestones

1. **Project scaffolding**: Create folder structure, config, and entrypoints
2. **Data preparation module**: Implement data loading/prep stubs
3. **LoRA/QLoRA modules**: Implement fine-tuning logic stubs
4. **CLI/REST API**: Entrypoint for triggering training jobs
5. **Dockerization**: Write Dockerfile, build scripts
6. **Azure Foundry deployment**: Write deployment stubs/scripts
7. **Testing**: Add test stubs for all modules

**Key Risks / Open Questions**:
- Azure Foundry specifics (container registry, deployment YAML, secrets)
- Data privacy/handling for training data
- Model artifact storage (local vs Azure Blob)

## Component/Dependency Diagram

![Diagram](https://dummyimage.com/600x300/cccccc/000000&text=Component+Diagram+Unavailable)

*(Mermaid diagram rendering failed; see code in source for intended structure.)*
