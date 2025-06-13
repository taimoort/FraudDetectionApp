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
   ``` 
## Architecture

                                   ┌──────────────────────┐
                                   │   React Front-end    │
                                   └──────────▲───────────┘
                                              │  HTTP POST /transaction
                                              │
                                   ┌──────────┴───────────┐
                                   │ TransactionService   │
                                   │       API            │
                                   └──────────┬───────────┘
                                              │   produce
                                              │   topic: transactions_raw
                                   ┌──────────▼───────────┐
                                   │     Apache Kafka     │
                                   └──────────┬───────────┘
                                              │
             Zookeeper config                 │
              ┌──────────┐                    │ consume
              │ Zookeeper│                    │
              └────▲─────┘                    ▼
                   │               ┌──────────────────────────┐
                   └──────────────►│ FraudDetector-Service    │
                                   │  Worker (.NET)           │
                                   │  ─ rules / enrichment    │
                                   └──────────┬───────────────┘
                                              │  INSERT (fraud)           Serilog logs
                                              │  into SQL                 ─────────────
                                              │                           │
                                              │ produce                   ▼
                                              │ topic: transactions_enriched
                                   ┌──────────▼───────────┐       ┌───────────────────┐
                                   │   Kafka Connect      │──────►│   Elasticsearch   │
                                   │ Elasticsearch Sink   │       └────────▲──────────┘
                                   └──────────▲───────────┘                │
                                              │                            │
                                              │                            │ Kibana dashboards
                                              │                            ▼
                                   ┌──────────┴───────────┐      ┌───────────────────┐
                                   │     Apache SQL       │      │      Kibana       │
                                   │ (PostgreSQL store)   │      └───────────────────┘
                                   └──────────────────────┘

                             ( All containers orchestrated via Docker Compose )