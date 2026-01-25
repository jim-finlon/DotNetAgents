# DotNetAgents Documentation

This directory contains all documentation for the DotNetAgents library, organized by category.

## 📁 Directory Structure

### `/architecture/`
Architecture and design documentation:
- `technical-specification.md` - Complete technical specification
- `package-strategy-analysis.md` - Package structure analysis
- `microsoft-agent-framework-analysis.md` - Analysis of Microsoft Agent Framework compatibility

### `/features/`
Feature-specific documentation:
- `/education/` - Educational extensions documentation
  - `REQUIREMENTS.md` - Educational package requirements
  - `TECHNICAL_SPECIFICATION.md` - Architecture and algorithms
  - `STATUS.md` - Implementation status
  - `TEST_COVERAGE.md` - Test coverage details
  - `EDUCATION_ENHANCEMENTS.md` - Education enhancements documentation
- `JARVIS_VISION.md` - JARVIS system vision and design

### `/guides/`
User guides and how-to documentation:
- `INTEGRATION_GUIDE.md` - Integration guide for tasks, knowledge, and workflows
- `API_REFERENCE.md` - API reference documentation
- `ERROR_HANDLING.md` - Error handling patterns and best practices
- `TESTING_STRATEGY.md` - Testing strategy and guidelines
- `BUILD_AND_CICD.md` - Build and CI/CD pipeline documentation
- `VERSIONING_AND_MIGRATION.md` - Versioning strategy and migration guides

### `/status/`
Project status documents:
- `PROJECT_STATUS.md` - Current development status and completed features
- `JARVIS_IMPLEMENTATION_STATUS.md` - JARVIS implementation tracking

### Root Level
Additional documentation files:
- `REQUIREMENTS.md` - Functional and non-functional requirements
- `TECHNICAL_SPECIFICATIONS.md` - Technical specifications
- `comparison.md` - Comparison with LangChain, LangGraph, and Microsoft Agent Framework
- `PACKAGE_METADATA.md` - Package metadata and NuGet information
- `PERFORMANCE_BENCHMARKS.md` - Performance benchmarks and metrics
- `GIT_WORKFLOW.md` - Git workflow and branching strategy
- `DEVELOPMENT_DATABASE.md` - Development database configuration
- `AISESSIONPERSISTENCE_COMPARISON.md` - Comparison with AiSessionPersistence project

### `/kubernetes/`
Kubernetes deployment documentation:
- `README.md` - Kubernetes deployment guide
- `DEPLOYMENT_SUMMARY.md` - Deployment summary and status
- `/manifests/` - Kubernetes manifest files
  - `namespace.yaml` - Namespace definition
  - `configmap.yaml` - Application configuration
  - `secrets.yaml.example` - Secrets template
  - `*-deployment.yaml` - Service deployments
  - `ingress.yaml` - Ingress configuration
- `/monitoring/` - Monitoring stack manifests
  - `prometheus-deployment.yaml` - Prometheus configuration
  - `grafana-deployment.yaml` - Grafana configuration
  - `loki-deployment.yaml` - Loki and Promtail configuration
- `/helm/` - Helm charts
  - `/teaching-assistant/` - TeachingAssistant Helm chart

## 🔍 Quick Links

- **Getting Started**: See the main [README.md](../README.md)
- **Architecture**: Start with `/architecture/ARCHITECTURE_SUMMARY.md` or `/architecture/technical-specification.md`
- **Integration**: See `/guides/INTEGRATION_GUIDE.md`
- **Comparison**: See `/comparison.md` for comparison with LangChain, LangGraph, and Microsoft Agent Framework
- **Kubernetes Deployment**: See `/kubernetes/README.md` for production deployment
- **Status**: Check `/status/PROJECT_STATUS.md` for current progress
- **Database Setup**: See `/DEVELOPMENT_DATABASE.md` for database configuration
