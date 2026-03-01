## Required Development Secrets

This project requires the following user secrets for local development:

dotnet user-secrets set "R2:AccessKey" "<your-access-key>"
dotnet user-secrets set "R2:SecretKey" "<your-secret-key>"
dotnet user-secrets set "R2:Endpoint" "https://<account-id>.r2.cloudflarestorage.com"
dotnet user-secrets set "R2:Bucket" "<bucket>"
dotnet user-secrets set "R2:PublicBaseUrl" "https://<bucket>.<account-id>.r2.dev"