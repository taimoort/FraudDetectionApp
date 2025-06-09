# Fraud Detection App

This repo contains the initial setup for a .NET 8 + Kafka + PostgreSQL + React fraud detection system.

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

## Running Locally

1. Start infrastructure:  
   ```bash
   docker-compose up -d
