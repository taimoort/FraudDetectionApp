# Fraud Detection App

This repo contains the initial setup for a .NET 9 + Kafka + PostgreSQL + React fraud detection system.

## Prerequisites

- Docker Desktop (Windows / macOS) or Docker Engine + docker-compose on Linux 

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
   git clone https://github.com/taimoort/FraudDetectionApp.git
   cd FraudDetectionApp
   docker-compose up -d --build   # spins up Kafka, ZK, Postgres, ES, Kibana *and* both .NET services + React UI
   ``` 
## Default endpoints

| Component / Service           | URL / Connection                                   | Notes                                   |
|-------------------------------|----------------------------------------------------|-----------------------------------------|
| **React Dashboard**           | <http://localhost:8080>                            | Lists latest fraud alerts               |
| **TransactionService API**    | <http://localhost:5003/>                           | Postman UI for `/api/transaction`       |
| **Kibana**                    | <http://localhost:5601>                            | Create index pattern `fraud-*`          |

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