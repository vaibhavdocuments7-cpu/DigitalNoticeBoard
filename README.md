# Digital Notice Board

Interactive notice-board application built with an ASP.NET Core MVC API,
React, Bootstrap, JWT authentication, and repository-based SQL Server
persistence.

## Architecture

```text
React frontend
    -> ASP.NET Core controllers
        -> Application services
            -> Repository interfaces
                -> SQL Server repositories
                -> In-memory fallback repositories
```

The backend projects are:

- `DigitalNoticeBoard.Domain`: entities, roles, and notice status.
- `DigitalNoticeBoard.Application`: DTOs, services, and repository contracts.
- `DigitalNoticeBoard.Infrastructure`: SQL Server, in-memory repositories,
  storage failover, and initial data.
- `DigitalNoticeBoard.Api`: controllers, JWT authentication, middleware, and
  application startup.

## Run locally

Requirements:

- .NET SDK 10
- Node.js 24 or a compatible current version
- SQL Server LocalDB is optional

Start the API:

```powershell
dotnet run --project .\backend\src\DigitalNoticeBoard.Api
```

Start the React development server in a second terminal:

```powershell
Set-Location .\frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

Development accounts:

| Role | Username | Password |
|---|---|---|
| Administrator | `admin` | `Admin@123` |
| Viewer | `viewer` | `Viewer@123` |

The viewer can only read published notices. The administrator can create,
edit, publish, archive, and delete notices.

## SQL and fallback behavior

The default connection uses SQL Server LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=DigitalNoticeBoard;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3
```

On startup, the API creates the database when possible. If SQL Server is not
installed, cannot be reached, or goes offline during a repository operation,
the API switches to thread-safe process memory. The interface displays the
active provider. In-memory changes are lost when the API restarts.

Set a different SQL connection through:

```powershell
$env:ConnectionStrings__NoticeBoard = "Server=...;Database=DigitalNoticeBoard;..."
```

Review the executable SQL in the `scripts` directory before applying it
manually.

## Production configuration

Do not use the development passwords in a deployed environment. Configure:

```text
DemoAccounts__AdminPassword
DemoAccounts__ViewerPassword
Jwt__Key
ConnectionStrings__NoticeBoard
```

`Jwt__Key` should be a long, randomly generated secret supplied by the
deployment platform. If it is omitted, the API generates an ephemeral key and
all tokens become invalid when the process restarts.

## Validation commands

```powershell
dotnet build .\DigitalNoticeBoard.sln
Set-Location .\frontend
npm run build
npm run lint
```
