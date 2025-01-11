
# Invoice Management System

This is a simple Invoice Management System built with a layered architecture, focusing on separation of concerns. It includes four distinct projects: API, Service, Data, and Test. The API serves as the entry point, and it communicates with the Service project for business logic and with the Data project for database interactions.

## Project Structure

### `InvoiceManagementSystem.API`
- Contains the API controllers (e.g., `InvoicesController`) and configurations for the API.
- The controller communicates with the Service project to handle requests.
- Does not directly interact with the database to follow the separation of concerns principle.

### `InvoiceManagementSystem.Service`
- Contains business logic and service classes related to invoice calculations and other operations.
- Implements reusable helper methods for business operations.
- Abstracted and reusable as a class library.

### `InvoiceManagementSystem.Data`
- Contains the Entity Framework Core DbContext and repository classes to interact with the database.
- Uses SQL Server for persistence.
- All database-related operations are encapsulated in repository classes (e.g., `InvoiceRepository.cs`).

### `InvoiceManagementSystem.Test`
- Contains unit tests and test cases for the service methods, adhering to test-driven development principles.

## API Documentation

- The API will be available at: `https://localhost:5001` (or the port specified in your `launchSettings.json`).
- Swagger UI for API documentation and testing can be accessed at: `https://localhost:5001/swagger`.

## Technologies Used
- **.NET 8.0**
- **Entity Framework Core 9.0**
- **AutoMapper 13.0**
- **SQL Server**
- **Swagger UI**

## Setup and Usage

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/InvoiceManagementSystem.git
   ```

2. **Navigate to the project directory:**
   ```bash
   cd InvoiceManagementSystem
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access the API:**
   Open `https://localhost:5001` for the API.

6. **Swagger UI:**
   Access Swagger UI at `https://localhost:5001/swagger` for interactive API documentation and testing.

## Contributing

Contributions are welcome! If you’d like to contribute, please follow these steps:
1. Fork the repository.
2. Create a new branch.
3. Make your changes and add tests.
4. Submit a pull request.

Please make sure to follow the coding conventions used in the project and ensure all tests pass before submitting your pull request.

