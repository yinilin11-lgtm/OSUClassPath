# OSU CoursePath

An AI-powered course information and degree planning assistant for Ohio State University students.

Live demo: https://osu-coursepath.onrender.com

## Overview

OSU CoursePath is a full-stack ASP.NET Core MVC web application that helps students explore Ohio State CSE course information, review BS CSE requirements, track saved courses, and ask an AI advisor course-planning questions. The app combines a structured local course catalog, account-based student workspaces, an admin dashboard, and Gemini-powered responses.

This repository is named `OSUClassPath`, but the project is presented as **OSU CoursePath** for a clearer portfolio title.

## Features

- Searchable OSU CSE course catalog with categories, tracks, credits, and prerequisites
- AI course advisor powered by Gemini API
- BS CSE requirement and curriculum context for advising responses
- User account creation and login
- Personal saved-course tracking
- Admin dashboard for reviewing registered users and saved course counts
- Bilingual interface support for English and Traditional Chinese
- Render deployment configuration with Docker

## Tech Stack

- C#
- ASP.NET Core MVC
- Razor Views
- JavaScript
- HTML / CSS
- Bootstrap
- SQLite
- Entity Framework Core
- ASP.NET Core Identity
- Gemini API
- Docker / Render

## Project Highlights

- Built as a full-stack academic advising portfolio project
- Uses MVC controllers, Razor views, Entity Framework Core, and Identity authentication
- Stores course, account, saved-course, and planning data in SQLite
- Connects a JavaScript chat interface to a C# backend API
- Uses local curriculum data to reduce hallucination and keep AI responses course-specific
- Deployed as a live web app through Render

## Project Structure

- `OSUClassPath/Controllers/` - Web request handling and application logic
- `OSUClassPath/Models/` - Course, user, chat, and planning data models
- `OSUClassPath/Views/` - Razor views for the web interface
- `OSUClassPath/Data/` - Entity Framework database context, seed data, and startup schema helpers
- `OSUClassPath/Migrations/` - Database migration files
- `OSUClassPath/wwwroot/` - Static CSS and JavaScript assets
- `Dockerfile` and `render.yaml` - Render deployment configuration

## Getting Started

### Prerequisites

- .NET SDK 10.0 or later
- Visual Studio 2026 or later
- Gemini API key

### Run Locally

Clone the repository:

```bash
git clone https://github.com/yinilin11-lgtm/OSUClassPath.git
cd OSUClassPath
```

Set local secrets:

```bash
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY" --project OSUClassPath
dotnet user-secrets set "Admin:Password" "YOUR_ADMIN_PASSWORD" --project OSUClassPath
```

Run the application:

```bash
dotnet run --project OSUClassPath/OSUClassPath.csproj
```

## Deployment

The project is configured for Render using Docker. Required Render environment variables:

- `Gemini__ApiKey`
- `Admin__Password`
- `ConnectionStrings__AdvisorDatabase`
- `ASPNETCORE_ENVIRONMENT`

## Future Improvements

- Add a degree planner with semester credit totals and prerequisite warnings
- Save AI chat sessions to each user account
- Add richer course recommendation logic
- Move production data storage from SQLite to a persistent database such as PostgreSQL
