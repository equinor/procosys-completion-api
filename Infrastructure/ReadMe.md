# Completion Api - Infrastructure with Pulumi

This repository manages the infrastructure resources for Completion Api using Pulumi.

## Overview

Pulumi is used to set up all azure resources needed to run completion API.
Deployment is handled via the radix setup



## Prerequisites

- [Pulumi](https://www.pulumi.com/docs/get-started/install/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (Ensure you're logged in with `az login`)
- .NET SDK (version 6 or higher)

## Initial Setup (First-Time Only)

This section is for the very first setup of the infrastructure.

1. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

2. **Initialize a new Pulumi stack**:
   ```bash
   pulumi stack init [stack-name, e.g., 'dev']
   ```

3. **Set Azure region**:
   ```bash
   pulumi config set azure-native:location <AzureRegion>
   ```
   > Replace `<AzureRegion>` with your desired Azure region, e.g., `norwayeast`.


## Setup for Next Developers

This section is for developers who are setting up after the initial configuration has been done.

1. **Ensure Prerequisites**: Make sure all prerequisites mentioned in the main README are installed.

2. **Login to Pulumi using Azure Storage**:
   ```bash
   pulumi login --cloud-url azblob://[your-storage-account-container-url]
   ```

3. **Select the existing Pulumi stack**:
   ```bash
   pulumi stack select [dev/test/prod]
   ```

## Infrastructure Deployment

1. **Preview infrastructure changes**:
   ```bash
   pulumi preview
   ```

2. **Apply infrastructure changes**:
   ```bash
   pulumi up
   ```
