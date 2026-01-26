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
- `VISUAL_WORKFLOW_DESIGNER.md` - Visual workflow designer guide
- `WORKFLOW_DESIGNER_UI.md` - Workflow designer UI guide
- `AI_POWERED_TOOLS.md` - AI-powered development tools guide
- `ADVANCED_MULTI_AGENT_PATTERNS.md` - Advanced multi-agent patterns guide
- `EDGE_COMPUTING.md` - Edge computing and mobile deployment guide
- `ECOSYSTEM_INTEGRATIONS.md` - Plugin architecture and marketplace guide
- `BEHAVIOR_TREES.md` - Behavior trees guide with examples and patterns
- `MESSAGE_BUSES.md` - Message bus implementations guide (Kafka, RabbitMQ, Redis, SignalR)
- `VECTOR_STORES.md` - Vector store comparison and usage guide
- `DOCUMENT_LOADERS.md` - Document loader comparison and usage guide
- `LLM_PROVIDERS.md` - LLM provider comparison and usage guide
- `ALERTING.md` - Prometheus alerting configuration
- `GRAFANA_DASHBOARDS.md` - Grafana dashboard setup and usage
- `LOAD_TESTING.md` - Load testing with NBomber
- `CHAOS_ENGINEERING.md` - Chaos engineering and resilience testing
- `CIRCUIT_BREAKERS.md` - Circuit breaker patterns
- `GRACEFUL_DEGRADATION.md` - Graceful degradation strategies

### `/status/`
Project status documents:
- `PROJECT_STATUS.md` - Current development status and completed features
- `JARVIS_IMPLEMENTATION_STATUS.md` - JARVIS implementation tracking

### `/examples/`
Example documentation and tutorials:
- `DISTRIBUTED_TRACING.md` - Distributed tracing examples and setup
- `PLUGIN_ARCHITECTURE.md` - Comprehensive plugin architecture examples
- `BEHAVIOR_TREE_INTEGRATION.md` - Behavior tree integration examples
- `STATE_MACHINE_INTEGRATION.md` - State machine integration examples

### `/operations/`
Operations and production documentation:
- `DISASTER_RECOVERY.md` - Disaster recovery procedures
- `RUNBOOK.md` - Operations runbook
- `CAPACITY_PLANNING.md` - Capacity planning guide

### `/community/`
Community and ecosystem documentation:
- `COMMUNITY_GUIDE.md` - Community resources and getting involved
- `DISCORD_SETUP.md` - Discord server setup guide
- `CONTRIBUTOR_RECOGNITION.md` - Contributor recognition program
- `SHOWCASE_GUIDELINES.md` - Project showcase guidelines

### `/education/`
Education and training documentation:
- `CERTIFICATION_PROGRAM.md` - Certification program overview
- `LEARNING_PATHS.md` - Structured learning paths
- `TRAINING_MATERIALS.md` - Training resources and materials

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
- `PHASE3_COMPLETE_SUMMARY.md` - Phase 3 completion summary
- `PHASE4_COMPLETE_SUMMARY.md` - Phase 4 completion summary
- `PHASE4_PROGRESS.md` - Phase 4 progress tracking
- `PHASE5_PROGRESS.md` - Phase 5 progress tracking
- `PHASE5_COMPLETE_SUMMARY.md` - Phase 5 completion summary

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

### Getting Started
- **Main README**: [README.md](../README.md)
- **Integration Guide**: `/guides/INTEGRATION_GUIDE.md`
- **API Reference**: `/guides/API_REFERENCE.md`
- **Plugin Architecture**: `/examples/PLUGIN_ARCHITECTURE.md`
- **Behavior Trees**: `/guides/BEHAVIOR_TREES.md`

### Production & Operations
- **Distributed Tracing**: `/examples/DISTRIBUTED_TRACING.md`
- **Alerting**: `/guides/ALERTING.md`
- **Grafana Dashboards**: `/guides/GRAFANA_DASHBOARDS.md`
- **Disaster Recovery**: `/operations/DISASTER_RECOVERY.md`
- **Operations Runbook**: `/operations/RUNBOOK.md`
- **Load Testing**: `/guides/LOAD_TESTING.md`
- **Chaos Engineering**: `/guides/CHAOS_ENGINEERING.md`

### Innovation Features
- **Visual Workflow Designer**: `/guides/VISUAL_WORKFLOW_DESIGNER.md`
- **AI-Powered Tools**: `/guides/AI_POWERED_TOOLS.md`
- **Advanced Multi-Agent**: `/guides/ADVANCED_MULTI_AGENT_PATTERNS.md`
- **Edge Computing**: `/guides/EDGE_COMPUTING.md`
- **Ecosystem**: `/guides/ECOSYSTEM_INTEGRATIONS.md`
- **Behavior Trees**: `/guides/BEHAVIOR_TREES.md`

### Provider & Integration Guides
- **Message Buses**: `/guides/MESSAGE_BUSES.md` - Kafka, RabbitMQ, Redis, SignalR
- **Vector Stores**: `/guides/VECTOR_STORES.md` - PostgreSQL, Pinecone, Weaviate, Qdrant, Chroma
- **Document Loaders**: `/guides/DOCUMENT_LOADERS.md` - PDF, CSV, Excel, EPUB, Markdown, etc.
- **LLM Providers**: `/guides/LLM_PROVIDERS.md` - OpenAI, Anthropic, Google, AWS, Local, etc.

### Community & Education
- **Community Guide**: `/community/COMMUNITY_GUIDE.md`
- **Certification**: `/education/CERTIFICATION_PROGRAM.md`
- **Learning Paths**: `/education/LEARNING_PATHS.md`

### Architecture & Development
- **Architecture**: `/architecture/ARCHITECTURE_SUMMARY.md`
- **Comparison**: `/comparison.md` - Comparison with LangChain, LangGraph, and Microsoft Agent Framework
- **Kubernetes Deployment**: `/kubernetes/README.md`
- **Status**: `/status/PROJECT_STATUS.md`
- **Database Setup**: `/DEVELOPMENT_DATABASE.md`
