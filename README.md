# Mini Finans Raporlama

A modern **financial tracking web application** built with **ASP.NET MVC, Entity Framework and SQL Server**.

The application allows users to manage income and expense transactions and visualize financial data through dashboards and charts.

---

## 🎬 Application Demo

<p align="center">
  <img src="Screenshots/MiniFinansApp.gif" width="800"/>
</p>

---

## ✨ Features

- Create, update and delete financial transactions
- Dashboard overview with financial summary
- Income / Expense visual analytics
- Category-based expense analysis
- Date and category filtering
- Transaction logging system
- Responsive user interface
- Chart-based data visualization

---

## 🛠 Technologies Used

### Backend
- ASP.NET MVC
- C#
- Entity Framework
- SQL Server
- LINQ

### Frontend
- Bootstrap 5
- Chart.js
- SweetAlert2
- Razor View Engine

---

## 🧠 Application Architecture

This project follows the **ASP.NET MVC architecture pattern**.

- **Controllers** → Handle HTTP requests and business flow  
- **Models** → Represent database entities  
- **Views** → User interface with Razor  

Entity Framework is used as an ORM for database operations.

---

## 📊 Dashboard Features

- Total income
- Total expenses
- Net balance
- Transaction count

Visual analytics:

- Income / Expense distribution chart
- Category-based expense chart

---

## 📸 Screenshots

### Dashboard

| Overview | Charts |
|----------|--------|
| ![](Screenshots/DashboardOverview.png) | ![](Screenshots/DashboardCharts.png) |

### Transactions

| Create | Edit |
|--------|------|
| ![](Screenshots/Create.png) | ![](Screenshots/Edit.png) |

| Details | Delete |
|--------|--------|
| ![](Screenshots/Details.png) | ![](Screenshots/Delete.png) |

---

## 🧩 ER Diagram

![ER Diagram](Screenshots/ERDiagram.png)

---

## 🗄 Database Schema

### Transactions

| Column | Description |
|--------|------------|
| Id | Primary key |
| Date | Transaction date |
| Type | Income / Expense |
| Category | Transaction category |
| Amount | Transaction amount |
| Description | Transaction description |
| CreatedAt | Creation timestamp |
| UpdatedAt | Last update timestamp |

---

### Logs

| Column | Description |
|--------|------------|
| Id | Primary key |
| Action | Performed action |
| Description | Log description |
| LogDate | Log timestamp |

---

## 🗃 Database Script


Database/MiniFinansRaporlama_DB.sql


Tables:
- Transactions
- Logs

---

## ⚙️ Installation

### 1. Clone repository
```bash
git clone https://github.com/MertcanKayirici/MiniFinansRaporlama.git
```
### 2. Open in Visual Studio
### 3. Create database
MiniFinansDB
### 4. Run SQL script
Database/MiniFinansRaporlama_DB.sql
### 5. Configure connection string
data source=YOUR_SERVER_NAME

Example:
data source=.
6. Run project

---

## 📁 Project Structure
MiniFinansRaporlama
│
├── Controllers
├── Models
├── Views
│
├── Database
│   └── MiniFinansRaporlama_DB.sql
│
├── Screenshots
│
├── README.md
├── Web.config
🎯 Learning Goals
ASP.NET MVC architecture
Entity Framework
CRUD operations
LINQ queries
Dashboard UI design
Data visualization
Logging systems

---

## 👨‍💻 Developer

Mertcan Kayırıcı
Hitit University – Computer Programming
