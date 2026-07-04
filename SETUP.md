# First-time setup notes

The repo ships without a generated EF Core migration (these are tool-generated
and shouldn't be hand-written). Generate it once after cloning:

```bash
# from the repo root
dotnet tool install --global dotnet-ef        # if you don't have it
dotnet restore

cd src/Questlog.Api
dotnet user-secrets init
dotnet user-secrets set "Igdb:ClientId" "<twitch-client-id>"
dotnet user-secrets set "Igdb:ClientSecret" "<twitch-client-secret>"
dotnet user-secrets set "Jwt:Secret" "<random-32+-char-string>"

# generate the initial migration against the DbContext
dotnet ef migrations add InitialCreate \
  --project ../Questlog.Infrastructure \
  --startup-project .

# create/update the database
dotnet ef database update \
  --project ../Questlog.Infrastructure \
  --startup-project .

dotnet run
```

Program.cs also calls `db.Database.Migrate()` on startup, so once the migration
exists, `docker compose up` will apply it automatically.

## Get IGDB credentials (free)
1. Go to https://dev.twitch.tv/console
2. Register an application → get Client ID + Client Secret
3. Those are your `Igdb:ClientId` / `Igdb:ClientSecret`
