## Containerized Blog

Blogging platform based on containerized microservices in ASP.NET Core

## TODO's

#### Web Gateway

- Implement authorization and authentication middleware
  - Read authority from env variable passed from docker-compose or k8s deploy
  - Verify token integrity with Identity Service
  - Add health checks

#### Blogging Service

- Implement authorization and authentication middleware
  - Skip verifying integrity, read user data
- Add domain events and their handlers
- Read hostname and port from env variables
- Add health checks
- Implement likes system
- Add DB migration

#### Comments Service

- Implement models, logic, infrastructure... ;)
- Read hostname and port from env variables
- Add health checks
- Add DB migration

#### Discovery Service

- Add health checks
- Review `ServiceInstanceKey`
- Add DB migration

#### Emailing Service

- Check if gRPC is a good fit for inter-services communication or go for events
- Expose Rest endpoint for sending custom emails by administrators to the users
- Read hostname and port from env variables
- Add health checks

#### Identity Service

- Implement authorization and authentication middleware
  - Skip verifying integrity, read user data
- Refactor and style Razor Pages
- Add avatar to profile pages
- Finish adding OpenIddict as IdP
  - Think about user id in JWT
  - Add OIDC Razor Pages (authorize, signout, userinfo, token, etc.)
  - Seed clients DB with frontend web client
  - Add scopes linked to the specific role and scopes management (?)
  - Add scopes of the frontend web client
- Check anti forgery token validation in Razor Pages
- Add Security Stamp validation on cookie reading
- Read hostname and port from env variables
- Add health checks
- Add DB migration and seed
#### Frontend

- Start working on it... ;)
- Possible stack: React, Vite (SSG?), Mantine
- Use auth data seeded in Identity Service to authenticate client

#### Docker Compose

- Add env variables in .env file consisting of service hostnames and ports
- Add networks so the databases of the specific services are accessible from these services only
- Pass connection strings as environment variables
- Add DB clusters, Redis clusters, RabbitMQ clusters
- Delete discovery service in favor of NGINX/HAProxy on top of multiple services and pass static strings to web gateway (?)

#### K8s

- Generate certs for ETCD cluster as an external DB
- Deploy K3s with Calico as an ingress
  - 2 Server nodes
  - 4 Agent nodes (?)

#### General

- Add commands validation using Fluent Validation
- Add tests
