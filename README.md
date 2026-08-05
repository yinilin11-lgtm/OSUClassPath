# OSU CoursePath

An AI-powered course information assistant designed for Ohio State University students.

## Overview

OSU CoursePath is a full-stack web application that helps students explore course information through a structured database and an AI-assisted chat interface. The project combines academic course records, student profile management, course tracking, and Gemini-powered responses to make course information easier to access and understand.

This repository is named `OSUClassPath`, but the project is presented as **OSU CoursePath** for a clearer and more professional portfolio title.

## Features

- Course information search and management
- AI-assisted course guidance
- Student profile management
- Student course tracking
- Structured course database
- Web-based advising interface
- Gemini API integration

## Tech Stack

- C#
- ASP.NET Core MVC
- Razor Views
- JavaScript
- HTML / CSS
- Bootstrap
- SQLite
- Entity Framework Core
- Gemini API

## Project Highlights

- Built as a full-stack academic advising portfolio project
- Uses MVC controllers, Razor views, and Entity Framework migrations
- Stores course and student planning data in a local SQLite database
- Connects a JavaScript chat interface to a C# backend API
- Demonstrates AI integration in an education-focused application

## Project Structure

- `OSUClassPath/Controllers/` - Handles web requests and application logic
- `OSUClassPath/Models/` - Defines course, student, and student-course data models
- `OSUClassPath/Views/` - Razor views for the web interface
- `OSUClassPath/Data/` - Entity Framework database context
- `OSUClassPath/Migrations/` - Database migration files
- `OSUClassPath/wwwroot/` - Static assets such as CSS and JavaScript

## Getting Started

### Prerequisites

- .NET SDK 10.0 or later
- Visual Studio 2026 or later

### Run Locally

Clone the repository:

```bash
git clone https://github.com/yinilin11-lgtm/OSUClassPath.git
cd OSUClassPath
```

Run the application:

```bash
dotnet run --project OSUClassPath/OSUClassPath.csproj
```

## Future Improvements

- Add user authentication
- Improve course recommendation logic
- Add degree requirement validation
- Include screenshots and a live demo link
- Expand the course dataset with more OSU course information
