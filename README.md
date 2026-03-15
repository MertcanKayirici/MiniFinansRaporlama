# Mini Financial Reporting

![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC-blue)
![C#](https://img.shields.io/badge/C%23-.NET-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-ORM-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-red)
![Bootstrap](https://img.shields.io/badge/Bootstrap-UI-purple)
![Chart.js](https://img.shields.io/badge/Chart.js-Visualization-orange)

Mini Financial Reporting is a **simple financial tracking web application** built using **ASP.NET MVC and Entity Framework**.

The application allows users to manage income and expense transactions, visualize financial data with charts, and analyze financial summaries through a dashboard interface.

This project demonstrates fundamental backend development skills including:

- CRUD operations
- Data visualization
- Filtering and querying
- Logging system implementation
- MVC architecture

---

# Features

• Add income and expense transactions  
• Edit and delete financial records  
• Dashboard financial summary  
• Chart-based data visualization  
• Category-based expense analysis  
• Date and category filtering  
• Transaction logging system  
• Responsive user interface  

---

# Technologies Used

## Backend

- ASP.NET MVC
- C#
- Entity Framework
- SQL Server

## Frontend

- Bootstrap 5
- Chart.js
- SweetAlert2

---

# Dashboard

The dashboard provides a quick overview of the user's financial data.

Users can view:

- Total income
- Total expenses
- Net balance
- Total transaction count

Additionally, the system includes data visualization components such as:

- Income vs Expense chart
- Expense distribution by category

These visualizations help users better understand their financial behavior.

---

# Database

The database schema used in this project is provided in the **Database** folder.


Database/MiniFinansRaporlama_DB.sql


This SQL script creates the following tables:

- **Transactions**
- **Logs**

### Transactions

Stores financial records including:

- date
- transaction type
- amount
- category
- description

### Logs

Stores system actions such as:

- transaction creation
- transaction updates
- transaction deletions

---

# Installation

Follow these steps to run the project locally.

### 1. Create a database in SQL Server


MiniFinansDB


### 2. Run the database script


Database/MiniFinansRaporlama_DB.sql


### 3. Configure the connection string

Update the **Web.config** connection string with your SQL Server name.

Example:


data source=YOUR_SERVER_NAME


or


data source=.


### 4. Run the project

Open the solution in **Visual Studio** and start the application.

---

# Screenshots

| Dashboard Overview | Dashboard Charts |
|-------------------|------------------|
| ![](MiniFinansRaporlama/Screenshots/DashboardOverview.png) | ![](MiniFinansRaporlama/Screenshots/DashboardCharts.png) |

| Create Transaction | Edit Transaction |
|-------------------|------------------|
| ![](MiniFinansRaporlama/Screenshots/Create.png) | ![](MiniFinansRaporlama/Screenshots/Edit.png) |

| Transaction Details | Delete Transaction |
|--------------------|--------------------|
| ![](MiniFinansRaporlama/Screenshots/Details.png) | ![](MiniFinansRaporlama/Screenshots/Delete.png) |

---

# Project Structure


MiniFinansRaporlama
│
├── Controllers
├── Models
├── Views
├── Database
│ └── MiniFinansRaporlama_DB.sql
│
├── Screenshots
│
├── README.md
├── Web.config


---

# Purpose

This project was developed to practice and demonstrate:

- ASP.NET MVC architecture
- Entity Framework integration
- CRUD operations
- Dashboard UI design
- Data visualization
- Filtering mechanisms
- Logging system development

---

# Developer

**Mertcan Kayırıcı**

Hitit University  
Computer Programming