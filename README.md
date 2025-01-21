## Database Migrations
- Postgres
	- `dotnet ef migrations add InitialCreate -c IdentityContext -p ../Deepin.Identity.Infrastructure -o Migrations/Identity` 
	- `dotnet ef migrations add InitialCreate -c ConfigurationContext -p ../Deepin.Identity.Infrastructure -o Migrations/Configuration` 
	- `dotnet ef migrations add InitialCreate -c PersistedGrantContext -p ../Deepin.Identity.Infrastructure -o Migrations/PersistedGrant` 