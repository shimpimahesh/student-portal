# Azure Key Vault Configuration Guide

This guide explains how to configure and use Azure Key Vault with the Student Portal API.

## Overview

Azure Key Vault is used to securely store and manage sensitive configuration data such as:
- JWT signing keys
- Database connection strings
- API keys and secrets
- Authentication credentials

## Prerequisites

- Azure Subscription
- Azure CLI installed locally
- .NET 9 SDK
- Appropriate permissions in Azure AD to create Key Vaults

## Setup Steps

### 1. Create an Azure Key Vault

Using Azure CLI:

```bash
# Login to Azure
az login

# Create a resource group
az group create --name <your-resource-group> --location eastus

# Create a Key Vault
az keyvault create --name <your-keyvault-name> --resource-group <your-resource-group> --location eastus
```

Replace `<your-keyvault-name>` with a unique name (3-24 alphanumeric characters).

### 2. Add Secrets to Key Vault

Add your application secrets:

```bash
# Set the signing key
az keyvault secret set --vault-name <your-keyvault-name> --name "Auth--SigningKey" --value "your-signing-key-here"

# Set database connection string (if applicable)
az keyvault secret set --vault-name <your-keyvault-name> --name "ConnectionStrings--DefaultConnection" --value "your-connection-string"

# Set the Azure Application Insights connection string
az keyvault secret set --vault-name <your-keyvault-name> --name "ApplicationInsights--ConnectionString" --value "your-application-insights-connection-string"

# Add other secrets as needed
az keyvault secret set --vault-name <your-keyvault-name> --name "Auth--Issuer" --value "StudentApi"
az keyvault secret set --vault-name <your-keyvault-name> --name "Auth--Audience" --value "StudentPortal"
```

**Note:** Use double hyphens (`--`) in secret names to represent hierarchical configuration sections.
The API loads the Application Insights connection string from this Key Vault secret after Key Vault configuration is added. Do not commit the connection string to `appsettings.json` or configure it with an environment variable when Key Vault is enabled.

### 3. Configure Access Permissions

#### For Local Development (Using Visual Studio / Visual Studio Code)

```bash
# Get your current user ID
az ad signed-in-user show --query id -o tsv

# Grant Key Vault access to your user
az keyvault set-policy --name <your-keyvault-name> \
  --object-id <your-user-id> \
  --secret-permissions get list
```

#### For Application (Service Principal)

If deploying to Azure App Service or Container Instances:

```bash
# Create a managed identity for your app service
az webapp identity assign --name <app-service-name> --resource-group <your-resource-group>

# Get the principal ID
PRINCIPAL_ID=$(az webapp identity show --name <app-service-name> --resource-group <your-resource-group> --query principalId -o tsv)

# Grant Key Vault access to the service principal
az keyvault set-policy --name <your-keyvault-name> \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### 4. Update Configuration Files

Update `appsettings.json` and `appsettings.Production.json`:

```json
{
  "AzureKeyVault": {
    "VaultUrl": "https://<your-keyvault-name>.vault.azure.net/"
  },
  "Auth": {
    "Issuer": "StudentApi",
    "Audience": "StudentPortal",
    "SigningKey": "local-development-signing-key-change-me-32-chars",
    "DemoEmail": "admin@studentportal.local",
    "DemoPassword": "ChangeMe123!"
  }
}
```

### 5. Local Development with DefaultAzureCredential

The application uses `DefaultAzureCredential` which automatically handles authentication in this order:

1. **Environment variables** - Service principal credentials
2. **Managed Identity** - When running on Azure (App Service, Container Instances, etc.)
3. **Visual Studio / Visual Studio Code** - Your signed-in Azure account
4. **Azure CLI** - Credentials from `az login`
5. **Interactive** - Browser-based authentication prompt

No further configuration is needed for local development if you've logged in with Azure CLI or Visual Studio.

## Secret Naming Conventions

Azure Key Vault uses the following naming conventions for configuration:

```
Configuration Section: Auth:SigningKey
Secret Name: Auth--SigningKey

Configuration Section: ConnectionStrings:DefaultConnection
Secret Name: ConnectionStrings--DefaultConnection

Configuration Section: AppSettings:ApiKey
Secret Name: AppSettings--ApiKey
```

Replace colons (`:`) with double hyphens (`--`).

## Environment-Specific Configuration

The application loads configuration in the following order (later values override earlier ones):

1. `appsettings.json` (default settings)
2. `appsettings.{Environment}.json` (environment-specific settings)
3. Environment variables
4. Azure Key Vault secrets
5. Command-line arguments

## Troubleshooting

### Issue: Access Denied error

**Solution:** Ensure you have proper permissions in Key Vault:

```bash
# Check current policies
az keyvault show --name <your-keyvault-name>

# Update policies if needed
az keyvault set-policy --name <your-keyvault-name> \
  --object-id <your-object-id> \
  --secret-permissions get list
```

### Issue: Secret not found

**Solution:** Verify the secret exists and naming matches configuration:

```bash
# List all secrets
az keyvault secret list --vault-name <your-keyvault-name>

# Get a specific secret
az keyvault secret show --vault-name <your-keyvault-name> --name "Auth--SigningKey"
```

### Issue: DefaultAzureCredential fails in local development

**Solution:** Ensure you're logged in:

```bash
# Azure CLI login
az login

# Or Visual Studio login (in VS: Tools > Options > Azure Service Authentication)
```

## Deployment to Azure

### Using Azure App Service

1. Enable Managed Identity on your App Service
2. Grant the managed identity access to Key Vault
3. Set the `AzureKeyVault:VaultUrl` in Application Settings (or leave it to be read from `appsettings.json`)
4. Deploy your application

### Using Azure Container Instances

1. Create a managed identity for your container instance
2. Grant the managed identity Key Vault access
3. Set environment variables for the container to override `AzureKeyVault:VaultUrl` if needed
4. Deploy the container

### Using Docker

For containerized deployments, pass the vault URL as an environment variable:

```bash
docker run -e "AzureKeyVault__VaultUrl=https://<your-keyvault-name>.vault.azure.net/" <your-image>
```

## Security Best Practices

1. **Never commit secrets** to source control
2. **Use managed identities** in Azure instead of storing credentials
3. **Rotate secrets regularly** - Set a reminder to update credentials periodically
4. **Use Key Vault access policies** to follow the principle of least privilege
5. **Enable audit logging** in Key Vault to track secret access
6. **Use separate Key Vaults** for different environments (dev, staging, production)
7. **Enable soft delete** and **purge protection** on Key Vaults to prevent accidental deletion

## Additional Resources

- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [DefaultAzureCredential Documentation](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential)
- [Azure.Extensions.AspNetCore.Configuration.Secrets](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/extensions/Azure.Extensions.AspNetCore.Configuration.Secrets)
