# Acme Global College – Student & Course Management System

ASP.NET Core MVC application for managing branches, courses, student enrolments, attendance, gradebook, and exam results.

## Default Users

| Role      | Email                | Password     |
|-----------|----------------------|--------------|
| Admin     | admin@vgc.com        | Admin123!    |
| Faculty   | faculty@vgc.com      | Faculty123!  |
| Student 1 | student1@vgc.com     | Student123!  |
| Student 2 | student2@vgc.com     | Student123!  |

## Setup Instructions

1. Clone the repository
2. Run `dotnet restore`
3. Run `dotnet ef database update` (from the VgcCollege.Web folder)
4. Run `dotnet run --project VgcCollege.Web`
5. Open `http://localhost:5000`

## Features

- Role‑based authentication (Admin, Faculty, Student)
- Branch, Course, Student, Enrolment management
- Staff assignments (Faculty → Course)
- Exam result release (toggle provisional/released)
- Attendance tracking (by week, per course)
- Gradebook (assignment results) – *in progress*
- Student views (grades, attendance, exam results) – *in progress*

## Technologies

- .NET 10.0
- ASP.NET Core MVC
- Entity Framework Core (SQLite)
- ASP.NET Core Identity
- Bootstrap 5
- Bogus (seed data)
- xUnit (tests)

## GitHub Actions

CI workflow runs on every push to main: restore, build, test.
