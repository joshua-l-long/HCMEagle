# 🚀 EAGLE Development Environment

## ✨ What is this?

A production-ready **full-stack starter kit** built with modern technologies and best practices:

- **Frontend**: Angular 20
- **Backend**: .NET 9 API with Clean Architecture
- **Database**: PostgreSQL
- **DevOps**: Docker, GitHub Actions, NGINX

## 🚀 Quick Start (Eagle .Net and Angular development)
<p>
!!!Note: all login credentials for Postgres and pgAdmin are contained in .env file.
If usernames and passwords are updated (recommended for production) they need to be updated in the backend connection string located at /backend/src/Contact.Api/appsettings.json. All other areas should reference the env variable.
</p>

```bash
# create .env file
cp .env.example .env

# Start .net angular eagle development services with Docker Compose
docker compose -f docker-compose.yml -f docker-compose.development.yml up
```

🔗 Then access:
- Frontend: 4200 port through ports tab
- API: 8000 port through ports tab
- PgAdmin: 5051 port through ports tab
- Postgres: 5432 port (attach sqlTools extention or use pgAdmin)
- Swagger: https://siteurl:8000/swagger

## 🚀 Quick Start (Eagle .Net and Angular production)

```bash
# create .env file if not already created
cp .env.example .env

# Start .net angular eagle development services with Docker Compose
docker compose up -d
```

🔗 Then access:
- Frontend: 80 port through ports tab
- PgAdmin: 5051 port through ports tab
- Postgres: 5432 port (attach sqlTools extention or use pgAdmin)

## 🚀 Quick Start (schema processor manual start)

```bash
# create .env file if not already created
cp .env.example .env

# Start the environment 
docker compose -f docker-compose.schemaprocessor.yml up -d
```

🔗 Then access:
- PgAdmin: 5051 port through ports tab
- Postgres: 5432 port (attach sqlTools extention or use pgAdmin)

### Access PostgreSQL Directly

```bash
docker exec -it postgres psql -U testuser -d Contacts
```

Or connect from your host:
- Host: `locaslhost`
- Port: `5432`
- Database: `db name from .env`
- Username: `username from .env`
- Password: `password from .env`

### 👤 Default Users for demo application

| Username | Password | Role |
|----------|----------|------|
| nitin27may@gmail.com | P@ssword#321 | Admin |
| editor@gmail.com | P@ssword#321 | Editor |
| reader@gmail.com | P@ssword#321 | Reader |


###Useful Docker Commands

## Rebuilding Containers

If you make changes to Dockerfiles, docker-compose.yml, or switching programs:

```bash
docker compose up --build
#if you need to run a program outside of main eagle application
docker compose -f docker-compose.filename.yml up --build
```

## Stopping the Environment

```bash
docker compose down
```

## To remove volumes (database data will be lost):

```bash
docker-compose down -v
```
## Other commands

```bash
# View container logs
docker logs frontend
docker logs api
docker logs db

# Access a container's shell
docker exec -it api /bin/bash
docker exec -it db psql -U postgres contacts

# Rebuild a specific service
docker compose build api
docker compose up -d api

#run specific docker-compose file
docker compose -f docker-compose.filename.yml
```