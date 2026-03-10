# Deployment Refactor Plan

## Goal

Refactor production deployment without changing the core hosting model:

- Keep the ASP.NET Core app and SQL Server on the same machine
- Keep both services running in Docker containers
- Reduce downtime during deploys
- Remove avoidable security and operational risks
- Make production deployment easier to reason about and recover

## Current State Summary

Today the deployment works, but it has a few structural weaknesses:

- The running app container is stopped before the replacement is proven healthy
- The app container is exposed directly on the host with forwarded headers trusted from any proxy
- The SQL Server port is published to the host even though the app and DB are on the same Docker network
- The app connects as the SQL Server `sa` account instead of a dedicated application login
- Database migrations run as part of web app startup
- Deployment is driven by ad hoc `docker run` commands instead of a single declarative stack
- The deployment documentation has drifted from the actual workflow

## Target Architecture

The target design stays simple:

- One production server
- One reverse proxy entrypoint
- One app container
- One SQL Server container
- One Docker Compose stack that defines the running system
- One internal Docker network for app to database traffic
- One named Docker volume for SQL Server data

Only the reverse proxy should publish host ports. The app and database should communicate over Docker networking only.

## Refactor Steps

### Step 1: Create a Production Compose Stack

Create a production Compose file that defines the app and SQL Server together.

Implementation:

- Add a `docker-compose.production.yml` or similar file at the repo root or in a dedicated deployment folder
- Define `cptcevents-app` and `sqlserver` in the same stack
- Put both services on a shared internal network
- Keep SQL Server data in a named volume
- Add restart policies to both services

Why this matters:

- The deployment becomes declarative instead of procedural
- The network, volume, container names, and runtime configuration live in one place
- Recovery becomes simpler because the running topology is explicit

Done when:

- A single `docker compose -f ... up -d` can bring up the whole production stack

### Step 2: Stop Publishing the Database Port

Remove the host port mapping for SQL Server unless there is a real operational need to connect from outside Docker.

Implementation:

- Remove `1433:1433` from the production SQL Server service definition
- Keep the SQL Server hostname reachable only within the Docker network
- If occasional admin access is needed, use a temporary tunnel, `docker exec`, or a one-off admin container on the same network

Why this matters:

- The database no longer listens on the host network
- This reduces unnecessary attack surface without changing the app architecture

Done when:

- The app can still connect to SQL Server by container hostname, but the host no longer exposes port `1433`

### Step 3: Stop Publishing the App Directly Unless Required

The app should usually only be reachable through the reverse proxy.

Implementation:

- If Caddy runs on the host, bind the app port to `127.0.0.1` instead of all interfaces
- If Caddy also runs in Docker, keep the app on an internal Docker network and let only Caddy publish ports
- Keep `UseForwardedHeaders`, but stop trusting all proxies unless the network boundary is truly locked down

Why this matters:

- It closes the gap between your forwarded-header trust model and actual network exposure
- It prevents bypassing the reverse proxy path

Done when:

- External traffic reaches the app only through the reverse proxy

### Step 4: Replace `sa` With a Dedicated Application Login

Create a SQL Server login and database user specifically for the app.

Implementation:

- Create a login such as `cptcevents_app`
- Grant only the permissions needed for normal application operation and migrations
- Store the app login password separately from the SQL Server admin password
- Update the production connection string to use the app login instead of `sa`

Why this matters:

- Compromise of the app container does not automatically expose full SQL Server administrative control
- Credential scope matches application responsibility more closely

Done when:

- The production app runs successfully without using `sa`

### Step 5: Add Health Checks and Readiness Gates

Make the deployment wait for services to become usable, not just started.

Implementation:

- Add a SQL Server health check to the production Compose definition
- Add an app health endpoint such as `/health` or a simple internal readiness endpoint
- Update deployment logic so the new app container is considered live only after the health check passes

Why this matters:

- A running container is not the same as a ready service
- Health checks let you fail fast before routing real traffic to a broken deployment

Done when:

- Deployment logic can detect whether SQL Server and the app are healthy before finalizing rollout

### Step 6: Move Migrations Out of Normal App Startup

Stop coupling schema changes directly to every web app boot.

Implementation:

- Create a separate migration step in deployment, or a one-shot migration container based on the app image
- Run migrations before switching traffic to the new app container
- Keep startup seeding logic only for data that is truly safe and idempotent

Why this matters:

- Startup failures become easier to diagnose
- Schema rollout becomes an explicit operation instead of hidden application side effect
- Rollback planning becomes clearer

Done when:

- The web container can start without being responsible for schema upgrades

### Step 7: Validate Secrets Before Touching Running Containers

Reorder deployment so validation happens before anything is stopped.

Implementation:

- Check all required secrets at the start of the workflow
- Validate Compose configuration before applying changes
- Only replace containers after all required inputs are present

Why this matters:

- Prevents avoidable downtime caused by a bad deploy input

Done when:

- Missing configuration fails the workflow before production is changed

### Step 8: Change the Deployment Flow From "Destroy Then Start" to "Start, Verify, Cut Over"

The deployment should prove the new revision works before it replaces the old one.

Implementation:

- Build the new image first
- Start the new container or service revision
- Wait for health checks to pass
- Only then stop the previous app container
- If your final topology does not allow two app containers at once on the same port, use the reverse proxy or container naming to control the cutover cleanly

Why this matters:

- It reduces deployment-caused downtime
- It gives you a clearer rollback point

Done when:

- A failed app start no longer takes production offline immediately

### Step 9: Separate Build From Production Host Runtime

Keep the self-hosted production machine focused on running containers rather than compiling source.

Implementation:

- Build the image in GitHub Actions
- Push the image to a registry such as GHCR
- Have the production machine pull the tagged image and restart the Compose stack

Why this matters:

- Reduces moving parts on the production server
- Makes deployments more reproducible
- Lets the server run known images instead of building from a mutable workspace

Done when:

- Production deploys consume prebuilt images rather than building locally on the server

### Step 10: Add Backup and Restore Procedures for SQL Server

Refactor is incomplete without a recovery path.

Implementation:

- Add scheduled SQL Server backups
- Store backups off the machine or in durable remote storage
- Document a restore procedure and test it periodically

Why this matters:

- Container persistence is not backup
- A named volume protects against container deletion, not host failure or data corruption

Done when:

- You can restore the database onto a fresh machine from recent backups

### Step 11: Update the Deployment Documentation

Bring the docs back in line with reality after the refactor lands.

Implementation:

- Update `DEPLOYMENT.md` to match actual container names, networks, and workflow steps
- Document where secrets live and how deployment is triggered
- Document the rollback path and backup location

Why this matters:

- Deployment documentation is only useful if it matches production exactly

Done when:

- A second person could deploy or recover the system using the docs alone

## Suggested Execution Order

If you want the least disruptive path, do the work in this order:

1. Create the production Compose stack
2. Validate secrets before container replacement
3. Remove direct database port exposure
4. Restrict direct app exposure to the reverse proxy path
5. Add health checks and readiness gates
6. Introduce a dedicated SQL Server app login
7. Move migrations into a separate deployment step
8. Change rollout to start, verify, and then cut over
9. Shift to prebuilt images from a registry
10. Add backups and restore documentation
11. Update `DEPLOYMENT.md`

## Minimum Viable Refactor

If you want the highest-value improvements first, do these before anything else:

1. Validate secrets before stopping the app
2. Move app and SQL Server into a single production Compose stack
3. Remove host exposure for SQL Server
4. Stop using `sa` for the application connection
5. Add a health check based rollout gate

## Final State Checklist

The refactor is complete when all of the following are true:

- Production is defined by a Compose stack instead of scattered `docker run` commands
- SQL Server is not exposed on the host unless explicitly needed
- The app is not directly reachable except through the reverse proxy path
- The app uses a dedicated SQL Server login
- Migrations are a separate deployment concern
- Deployments validate inputs before touching live containers
- Deployments use service health, not just container existence
- Backups exist and restore has been tested
- The documentation matches the real system