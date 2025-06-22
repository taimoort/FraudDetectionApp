# Fraud Detection App

<img alt=".NET9" src="https://img.shields.io/badge/.NET-9.0-green"/><img alt="Kafka" src="https://img.shields.io/badge/Kafka-3.3-blue"/><img alt="Docker" src="https://img.shields.io/badge/Docker-ready-lightblue"/> 
[![build-test](https://github.com/taimoort/FraudDetectionApp/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/taimoort/FraudDetectionApp/actions/workflows/ci.yml)
[![docker build-and-push](https://github.com/taimoort/FraudDetectionApp/actions/workflows/docker-build.yml/badge.svg?branch=main)](https://github.com/taimoort/FraudDetectionApp/actions/workflows/docker-build.yml)

This repo contains the initial setup for a .NET 9 + Kafka + PostgreSQL + React fraud detection system.

## Prerequisites

- Docker Desktop (Windows / macOS) or Docker Engine + docker-compose on Linux 

## Features

- ✔ Kafka producer in TransactionService
- ✔ Kafka consumer + fraud rule
- ✔ Kafka Connect → Elasticsearch
- ✔ EF Core PostgreSQL persistence
- ✔ Serilog logs → Elasticsearch
- ✔ Schema Registry + Avro for versioned event contracts
- ✔ Kibana log visualization and discovery
- ✔ GitHub Actions** CI: build backend & frontend, push Docker images to **Docker Hub**

## Running Locally

1  Run everything in containers

   ```bash
   # 1. Clone the repo from GitHub
   git clone https://github.com/taimoort/FraudDetectionApp.git
   cd FraudDetectionApp

   # 2. Pull and start pre-built images (no .NET SDK or other build toolchain required)
   docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

   # 🛠 Quick test command
   curl -X POST http://localhost:5003/api/transaction -H "Content-Type:application/json" -d '{ "transactionId":"a1b2c3d4-e5f6-1234-abcd-1234567890ab","amount":15000,"timestamp":"2025-06-18T17:07:00Z"}' 
   ``` 
## Default endpoints

| Component / Service           | URL / Connection                                   | Notes                                                     |
|-------------------------------|----------------------------------------------------|-----------------------------------------------------------|
| **React Dashboard**           | <http://localhost:8080>                            | Lists latest fraud alerts                                 |
| **TransactionService API**    | <http://localhost:5003/>                           | Postman UI for `/api/transaction`                         |
| **Kibana**                    | <http://localhost:5601>                            | Create index pattern `transactions, transactions_enriched`|

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
