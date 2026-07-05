# 🏺 Pharaohs Legacy

An interactive **ASP.NET Core MVC** web application that allows users to discover Ancient Egyptian civilization through an engaging digital experience. The platform combines historical content, museum exploration, intelligent search, and an AI-powered tour guide to provide an immersive educational journey.

---

## 📖 Overview

Pharaohs Legacy is designed to preserve and present the history of Ancient Egypt in a modern web application.

Users can browse famous Pharaohs, temples, museums, artifacts, and dynasties while interacting with an AI assistant capable of answering historical questions.

---

# ✨ Features

### 👤 Authentication
- User Registration
- User Login
- Guest Mode
- Session-based Authentication

### 🏺 Historical Content
- Pharaohs
- Temples
- Museums
- Dynasties
- Artifacts
- Egyptian Gods
- Historical Timeline

### ❤️ User Experience
- Search
- Favorites
- Reviews & Ratings
- Helpful Reviews
- User Profile
- Interactive Egypt Map

### 🎫 Booking System
- Book Museum Visits
- Manage Reservations
- Payment Records

### 🛠 Admin Panel
- Dashboard
- Full CRUD Operations
- Content Management
- User Management

### 🤖 AI Tour Guide
- AI Chat Assistant
- Powered by Groq API
- LLaMA 3.1 Model
- Answers questions about Ancient Egypt

---

# 🏗 Built With

| Technology | Description |
|------------|-------------|
| ASP.NET Core MVC (.NET 8) | Web Framework |
| Entity Framework Core | ORM |
| SQL Server LocalDB | Database |
| Razor Views | UI |
| C# | Backend |
| HTML5 | Markup |
| CSS3 | Styling |
| JavaScript | Client-side |

---

# 📂 Project Structure

```
PharaohsLegacy
│
├── Controllers
├── Models
├── Views
├── Services
├── Migrations
├── Database
├── wwwroot
├── Program.cs
└── appsettings.json
```

---

# 🗄 Database

The application uses **SQL Server LocalDB** with **Entity Framework Core**.

### Create the database

```bash
dotnet ef database update
```

or execute

```
Database/Database.sql
```

to restore the complete database.

---

# 🚀 Getting Started

## Clone Repository

```bash
git clone https://github.com/kamal-aldeen/pharaohsLegacy.git
```

## Navigate

```bash
cd pharaohsLegacy
```

## Restore Packages

```bash
dotnet restore
```

## Update Database

```bash
dotnet ef database update
```

## Run

```bash
dotnet run
```

or simply press **F5** in Visual Studio.

---

# ⚙ Configuration

Update **appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },

  "Groq": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

> **Important**
>
> Never upload your API Keys or secrets to GitHub.

---

# 📸 Application Modules

- Home
- Pharaohs
- Temples
- Museums
- Dynasties
- Artifacts
- Gods
- Timeline
- Favorites
- Bookings
- Payments
- Reviews
- User Dashboard
- Admin Dashboard
- AI Tour Guide

---

# 💡 Future Improvements

- AI Trip Planner
- Recommendation System
- Multi-language Support
- Email Verification
- Two-Factor Authentication
- OAuth Login
- Cloud Deployment
- Mobile Responsive Improvements

---

# 📜 License

This project was developed for educational purposes and portfolio demonstration.

---

## ⭐ Support

If you found this project useful, consider giving it a **Star** ⭐ on GitHub.
