# SETUP GUIDE for Supermarket Project

## .NET 8 Setup Instructions

1. **Install .NET 8**:
   - Download the .NET 8 SDK from the [official website](https://dotnet.microsoft.com/download/dotnet/8.0).
   - Follow the installation instructions for your operating system.

2. **Verify the Installation**:
   - Open a terminal/command prompt and run:
     ```bash
     dotnet --version
     ```

## SQL Server 2022 Setup Instructions

1. **Install SQL Server 2022**:
   - Download SQL Server 2022 from the [Microsoft website](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).
   - Follow the installation steps provided.

2. **Configure SQL Server**:
   - Set up a new database for the Supermarket project.
   - Ensure SQL authentication is enabled.

## Docker Setup (Optional)

1. **Install Docker**:
   - Download Docker Desktop from the [official website](https://www.docker.com/products/docker-desktop).
   - Follow the installation instructions for your operating system.

2. **Run SQL Server in Docker**:
   - Use the following command to pull the SQL Server 2022 image:
     ```bash
     docker pull mcr.microsoft.com/mssql/server:2022-latest
     ```
   - Run SQL Server in a container:
     ```bash
     docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Your_password123' -p 1433:1433 --name sql_server -d mcr.microsoft.com/mssql/server:2022-latest
     ```

## Local Development Options

1. **Clone the Repository**:
   - Clone the Supermarket repo using:
     ```bash
     git clone https://github.com/RC-inventrix/Supermarket.git
     ```

2. **Navigate to the Project Directory**:
   - Change into the directory:
     ```bash
     cd Supermarket
     ```

3. **Run the Application**:
   - Begin development with:
     ```bash
     dotnet run
     ```
     - This assumes you have restored the necessary packages using `dotnet restore`.

## Conclusion

- Ensure you follow the instructions sequentially.
- If you encounter any issues, consult the README or the GitHub discussions.