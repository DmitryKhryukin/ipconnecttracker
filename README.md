How to run:

docker compose up --build
dotnet run --project IpConnectTracker.WriterService

Pontetial issues:
1. Error "IpConnectTracker.WriterService.DataAccess.PostgreSQL/Migrations is not shared from the host and is not known to Docker"

Steps to fix on MacOS:

- Open Docker Desktop;
- Go to Settings;
- Navigate to Resources → File Sharing;
- Click "Browse", select your project folder "../ipconnecttracker" and then the “+” button;
- Click "Apply & Restart" button.
