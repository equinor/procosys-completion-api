# procosys-completion-api
REST API for the completion module in Project Completion System (ProCoSys (PCS))

TODO Document setup #103905

# Local Docker Development Setup

This guide will help you set up your local environment to run the application using Docker Compose.

## Prerequisites

- Docker and Docker Compose installed.
- Access to ProCoSys Official NuGet feed.
- Access to Azure Container Registry.

## Configuration

Before running the application, you need to set up the following:

### 1. Secret.json

Ensure you have a populated `secret.json` created from Equinor.ProCoSys.Completion.WebApi.
Either with full setup, or minimal.
Ask a colleague for a copy if you dont have one.

### 2. Environment Variables

Create a `.env` file in the src directory (next to docker-compose) with the following content:

FEED_TOKEN= `ACCESSTOKEN`

![img.png](img.png)

The .env file should never be checked in to source control. As long as its placed in the src folder, it will be ignored by git.
But always double check before committing.

Replace `ACCESSTOKEN` with your actual access key to the ProCoSys Official NuGet feed.
We use Personal AccessTokens and the token needs the permission `Packaging (read)`.
This means that you give the container permission to access the feed on your behalf.
If you dont already have one, it can be created here: https://statoildeveloper.visualstudio.com/_usersSettings/tokens

### 3. Login to Azure Container Registry

Follow the guide provided [here](https://github.com/equinor/procosys-infra/tree/master/db-dev) to log in to the Azure Container Registry from your command window.

### 4. Update Database Connection String

Modify the `server` part of the database connection string to use `db` instead of `127.0.0.1` .
You find the connection string in the `secret.json`. Change the value of  `"ConnectionStrings:CompletionContext":` 

from `Server=127.0.0.1;Database=pcs-co...`

to `Server=db;Database=pcs-co...`


## Running the Application

Once all configuration is done, you can start the application by running the following command from the startup project:

`cd src/Equinor.ProCoSys.Completion.WebApi`
and then

`docker compose up`


This will pull the necessary images, build the services, and start the application. You can access it once it's up and running.

## 03.11.2023: Create the Database inside the container 
Until the sandbox image is updated with the completion database,
you will have to create the database manually after spinning up the container.
This means the application may not run correct the first time.
To create it, connect to Sql server using MsSql Management Studio, right click Databases folder and select New Database.
Create a db called pcs-completion-dev-db, then rerun docker compose up.