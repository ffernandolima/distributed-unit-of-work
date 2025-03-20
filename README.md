# DistributedUnitOfWork

DistributedUnitOfWork is a simple .NET application demonstrating distributed and ambient transactions coordinated via a two-phase commit (2PC) protocol. The app uses a unit-of-work pattern to manage transactions across two different databases—MS SQL Server and PostgreSQL—leveraging .NET's TransactionScope to coordinate a distributed transaction.

## Features

- **Distributed Transactions with 2PC:**  
  Uses TransactionScope to coordinate transactions across multiple resource managers.
  
- **Ambient Transactions:**  
  Automatically enlists connections in the ambient transaction when available.
  
- **Unit-of-Work Pattern:**  
  Provides an abstraction over database operations, ensuring all operations either commit or roll back together.

- **Database Seeders:**  
  Idempotent SQL scripts initialize the required tables on both databases.

- **Demonstration Service:**  
  A sample distributed service that inserts data into both databases within a single distributed transaction.

## Prerequisites

- [.NET 8 SDK or later](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://docs.docker.com/compose/install/)  
  (to run the database containers)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/ffernandolima/distributed-unit-of-work.git
cd distributed-unit-of-work
```

### 2. Configure Databases

The project uses a docker-compose.yml to run the required database containers:

```yml
services:
  mssql:
    image: mcr.microsoft.com/mssql/server:2019-latest
    container_name: mssql
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    networks:
      - backend

  postgres:
    image: postgres:13
    container_name: postgres
    environment:
      - POSTGRES_PASSWORD=YourStrong!Passw0rd
      - POSTGRES_USER=postgres
      - POSTGRES_DB=testdb
    ports:
      - "5432:5432"
    command: ["postgres", "-c", "max_prepared_transactions=100"]
    networks:
      - backend

networks:
  backend:
    driver: bridge
```

> **Note:** The PostgreSQL container is configured with `max_prepared_transactions=100` to enable prepared transactions needed for 2PC.

Run the containers using:

```bash
docker-compose up -d
```

### 3. Configure Connection Strings

Update the `appsettings.json` file with your connection strings:

```json
{
  "ConnectionStrings": {
    "MsSql": "Server=localhost,1433;Database=master;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;",
    "Postgres": "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=YourStrong!Passw0rd;"
  }
}
```

### 4. Run the Application

From the project root, run:

```bash
dotnet run
```

The application will:

1. Seed the databases by creating the necessary tables.
2. Execute a distributed transaction that inserts data into both MS SQL Server and PostgreSQL.
3. Demonstrate both commit and rollback scenarios.

## How It Works

- Unit of Work (UoW):
The IUnitOfWork interface defines methods to begin, commit, and roll back transactions. Implementations for MS SQL Server and PostgreSQL enlist in the ambient transaction when one exists.

- DistributedLinkedUnitOfWork:
This composite UoW uses TransactionScope to coordinate transactions across both databases, ensuring atomicity using a two-phase commit protocol.

- DistributedService:
A sample service that uses the distributed UoW to perform operations against both databases. It demonstrates the insertion of data and error handling to force rollback.

## Considerations

- Performance:
Distributed transactions can introduce overhead. Use them judiciously based on your application's consistency requirements.

- Ambient Transactions in Web APIs:
When using TransactionScope in web applications, be mindful of async flow (use `TransactionScopeAsyncFlowOption.Enabled`) and ensure that transactions are kept short to avoid resource contention.

## License

This project is licensed under the MIT License. See the LICENSE file for details.