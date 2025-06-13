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
   %% Fraud-Detection Architecture – Mermaid %%
graph TD
  subgraph Client
    User[User / Recruiter\n(web browser)]
  end

  subgraph Frontend
    ReactUI[React Dashboard\n(Vite + Nginx)]
  end

  subgraph Backend
    TransactionAPI[TransactionService<br/>(ASP.NET 8 API)]
    FraudDetector[FraudDetectorService<br/>(.NET 8 Worker)]
    Postgres[(PostgreSQL<br/>fraud_db)]
  end

  subgraph Kafka_Cluster
    ZK[Zookeeper]
    KafkaBroker[Kafka Broker]
    SchemaReg[Schema Registry]
    KafkaConnect[Kafka Connect<br/>(ES Sink)]
    KafkaRaw[(Topic:<br/>transactions_raw)]
    KafkaEnriched[(Topic:<br/>transactions_enriched)]
  end

  subgraph Observability
    ES[(Elasticsearch)]
    Kibana[Kibana<br/>Dashboards]
  end

  %% Client ⇄ Frontend
  User -- "HTTP (🛠 CRUD /transactions)" --> ReactUI

  %% Frontend → API
  ReactUI -- "POST /transaction" --> TransactionAPI

  %% API → Kafka (raw)
  TransactionAPI -- "Produce" --> KafkaRaw

  %% Fraud detector consumes raw
  KafkaRaw -- "Consume" --> FraudDetector

  %% Fraud detector enriches + logs
  FraudDetector -- "Enriched event" --> KafkaEnriched
  FraudDetector -- "Serilog sink" --> ES
  FraudDetector -- "INSERT\n(isFraud)" --> Postgres

  %% Kafka Connect streams enriched → ES
  KafkaEnriched -- "Kafka Connect Sink" --> KafkaConnect
  KafkaConnect -- "Bulk index" --> ES

  %% ES visualised by Kibana
  ES --> Kibana

  %% Support lines
  ZK -. manages .- KafkaBroker
  SchemaReg -. registers .- KafkaRaw
  SchemaReg -. registers .- KafkaEnriched
