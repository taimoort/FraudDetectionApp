# Fraud Detection App

This repo contains the initial setup for a .NET 9 + Kafka + PostgreSQL + React fraud detection system.

## Prerequisites

- Docker Desktop (Apple Silicon)  
- .NET 9 SDK  
- Node.js (LTS)

## Features

- ✔ Kafka producer in TransactionService
- ✔ Kafka consumer + fraud rule
- ✔ EF Core PostgreSQL persistence
- ✔ Serilog logs → Elasticsearch
- ✔ Kibana log visualization and discovery
- ✔ GitHub Actions** CI: build backend & frontend, push Docker images to **Docker Hub**

## Running Locally

1  Run everything in containers

   ```bash
   git clone https://github.com/<your-user>/fraud-detection-app.git
   cd fraud-detection-app
   docker-compose up -d --build   # spins up Kafka, ZK, Postgres, ES, Kibana *and* both .NET services + React UI


## Architecture

```mermaid
%% Fraud-Detection Architecture
graph TD
  %% Clients
  user[User / Recruiter]
  subgraph Front-end
    ui[React Dashboard]
  end
  subgraph Back-end
    api[TransactionService (API)]
    worker[FraudDetectorService]
    pg[(PostgreSQL)]
  end
  subgraph Kafka
    zk[Zookeeper]
    broker[Kafka Broker]
    raw[(transactions_raw)]
    enriched[(transactions_enriched)]
    connect[Kafka Connect]
    registry[Schema Registry]
  end
  subgraph Observability
    es[(Elasticsearch)]
    kib[Kibana]
  end

  user -->|HTTP| ui
  ui -->|POST /transaction| api
  api -->|produce| raw
  raw -->|consume| worker
  worker -->|produce| enriched
  enriched -->|sink| connect
  connect --> es
  worker -->|Serilog| es
  worker --> pg
  es --> kib
  zk -.-> broker
  registry -.-> raw
  registry -.-> enriched
