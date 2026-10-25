# Dormitory Management System

A web-based dormitory management application developed using **ASP.NET Core Razor Pages, C#, Entity Framework Core, and MySQL**.

This project helps manage student information, dormitory rooms, room assignments, and payment records in one centralized application.

## Features

### Student Management
- View student records.
- Register new students.
- Edit student information.
- Protect students' room assignment history by preventing deletion when assignment records exist.

### Room Management
- View dormitory rooms.
- Add new rooms.
- Edit room information.
- Delete rooms when permitted.

### Room Assignment Management
- Assign students to rooms.
- View assignment records.
- Edit existing assignments.
- Cancel active assignments.
- Prevent students from having multiple active room assignments.
- Check room capacity before assigning students.

### Payment Management
- Record rental, water, and electricity fees.
- Record payment dates, methods, and statuses.
- Edit and delete payment records.
- View payment records and payment reports.

## Technologies

- C#
- ASP.NET Core Razor Pages
- Entity Framework Core
- MySQL
- MySQL Workbench
- JetBrains Rider
- Git and GitHub

## Project Structure

```text
DormitoryManagement/
├── Data/
│   └── DormitoryDbContext.cs
├── Models/
│   ├── Student.cs
│   ├── Room.cs
│   ├── RoomAssignment.cs
│   └── Payment.cs
├── Pages/
│   ├── Students/
│   ├── Rooms/
│   │   └── RoomAssignments/
│   ├── Payments/
│   └── Dashboard.cshtml
├── Properties/
├── wwwroot/
├── .gitignore
├── DormitoryManagement.csproj
├── Program.cs
└── README.md
```

## Getting Started

### Prerequisites

Install the following:

- .NET SDK compatible with the project.
- MySQL Server.
- JetBrains Rider or another compatible .NET IDE.
- MySQL Workbench (recommended).

### 1. Clone the repository

```bash
git clone https://github.com/Helena2648/DormitoryManagement.git
cd DormitoryManagement
```

### 2. Prepare the database

Create or prepare a MySQL database named `dormitory_db`.

Ensure that the required tables exist and match the application's models. The database schema may need to be prepared separately.

### 3. Configure the connection string

Create a local `appsettings.json` file in the project directory and configure your own MySQL connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=dormitory_db;User=YOUR_MYSQL_USER;Password=YOUR_MYSQL_PASSWORD;"
  }
}
```

Replace the placeholder username and password with your own database credentials.

**Security:** Never commit real database credentials or access tokens to GitHub. Keep your local `appsettings.json` excluded by `.gitignore`.

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Run the application

```bash
dotnet run
```

Open the local URL displayed in the terminal.

## Future Improvements

- Student deactivation and reactivation.
- Enhanced payment reports.
- User authentication and role-based access.
- Improved room occupancy summaries.
- Automated testing.

## Author

**Helena**

GitHub: [@Helena2648](https://github.com/Helena2648)

---

