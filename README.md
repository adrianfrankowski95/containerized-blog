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

#### Comments Service
- Implement models, logic, infrastructure... ;)
- Read hostname and port from env variables
- Add health checks

#### Discovery Service
- Add health checks
- Review `ServiceInstanceKey`

#### Emailing Service
- Check if gRPC is a good fit for inter-services communication or go for events solution
- Expose Rest endpoint for sending custom emails by administrators to the users
- Read hostname and port from env variables

#### Identity Service
- Implement authorization and authentication middleware
  - Skip verifying integrity, read user data
- Finish adding OpenIddict as IdP
  - Think about user id in JWT
  - Add Rest endpoints (authorize, signout, userinfo, token, etc.)
- Read hostname and port from env variables
  
#### Frontend
- Start working on it... ;)
- Possible stack: React, Vite (SSG?), Mantine

#### Docker Compose
- Add env variables in .env file consisting of service hostnames and ports
- Expose only the port specified in env file, eg: `"${RABBITMQ_PORT}"`
- Add links property for services, eg.: `"rabbitmq:${RABBITMQ_HOSTNAME}"`
- Pass hostnames and ports to the underlying Dotnet applications
- Add several replicas per each service
- Add multiple DB services

#### K8s
- Generate certs for ETCD cluster as an external DB
- Deploy K3s with Calico as an ingress
  - 2 Server nodes
  - 4 Agent nodes (?)
