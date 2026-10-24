# Interview-CCS

**Stack:** SQL Server · .NET 10 (C#, Dapper, JWT) · Angular 17 · Bootstrap 5.3

## Requisitos

- SQL Server en el puerto 1433 con autenticación de Windows (probado con SQL Server 2025 Developer)
- .NET SDK 10
- Node.js 20 o superior

## Base de datos

```powershell
sqlcmd -S "localhost,1433" -E -f 65001 -i db/01_schema.sql
sqlcmd -S "localhost,1433" -E -f 65001 -i db/02_procedures.sql
sqlcmd -S "localhost,1433" -E -f 65001 -i db/03_seed.sql
```

## API

```powershell
cd backend/CCS.Entities.Api
dotnet user-secrets set "Jwt:Key" "una-clave-secreta-de-al-menos-32-caracteres"
dotnet run
```

Queda en `https://localhost:7180`; la documentación interactiva (Scalar) en `/scalar/v1`.

Usuario de prueba: `admin` / `Admin123!`

## Frontend

```powershell
cd frontend
npm install
npm start
```

Queda en `http://localhost:4200`. Al iniciar sesión: `admin` / `Admin123!`

## Estructura

- `backend/` — API .NET 10 con CRUD sobre procedimientos almacenados
- `frontend/` — aplicación Angular 17 con tabla y formularios Bootstrap
- `db/` — esquema, procedimientos y datos iniciales
- `docs/` — diagrama entidad-relación y contrato OpenAPI

## Documentación

- Diagrama entidad-relación: [docs/diagrama-er.pdf](docs/diagrama-er.pdf)
- Contrato OpenAPI: `docs/openapi.json`
