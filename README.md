# 💰 Mini Finans Raporlama

A modern and dynamic **financial tracking web application** built with **ASP.NET MVC, Entity Framework, and SQL Server**.

This project allows users to manage income and expense transactions while providing powerful **dashboard analytics and visual reports**.

---

## 🎬 Demo

> Financial dashboard and transaction management system in action

![Mini Finans Demo](Screenshots/MiniFinansApp.gif)

---

## ✨ Key Features

- 💸 Income & Expense Management  
- 📊 Dashboard with Financial Summary  
- 📈 Chart-based Data Visualization  
- 🗂️ Category-based Expense Analysis  
- 🔍 Date & Category Filtering  
- 🧾 Transaction Logging System  
- 🎨 Responsive UI (Bootstrap 5)  
- ⚡ Fast and clean user experience  

---

## 🛠️ Tech Stack

- ASP.NET MVC (.NET Framework)  
- Entity Framework  
- Microsoft SQL Server  
- Bootstrap 5  
- Chart.js  
- SweetAlert2  
- HTML5 / CSS3  

---

## 🎥 Feature Demonstrations

### 📊 Dashboard Overview
![Dashboard](Screenshots/DashboardOverview.png)

### 📈 Financial Charts
![Charts](Screenshots/DashboardCharts.png)

### ➕ Create Transaction
![Create](Screenshots/Create.png)

### ✏️ Edit Transaction
![Edit](Screenshots/Edit.png)

---

## 📸 Screenshots

### 📊 Dashboard

| Overview | Charts |
|----------|--------|
| ![](Screenshots/DashboardOverview.png) | ![](Screenshots/DashboardCharts.png) |

---

### 💸 Transactions

| Create | Edit |
|--------|------|
| ![](Screenshots/Create.png) | ![](Screenshots/Edit.png) |

| Details | Delete |
|--------|--------|
| ![](Screenshots/Details.png) | ![](Screenshots/Delete.png) |

---

## 🧠 Database Design

![ER Diagram](Screenshots/ERDiagram.png)

---

## ⚡ Highlight Feature

One of the key features of this project is the **interactive financial dashboard**.

Users can:

- Instantly see income vs expense distribution  
- Analyze category-based spending  
- Track financial trends visually  

This improves decision-making and user experience.

---

## ⚙️ Installation

### 1. Clone the repository
```bash
git clone https://github.com/MertcanKayirici/MiniFinansRaporlama.git
2. Open the project
```

Open the solution file (.sln) in Visual Studio.

3. Create database

Create a database named:

MiniFinansDB
4. Run SQL script

Execute:

Database/MiniFinansRaporlama_DB.sql
5. Configure connection string
<connectionStrings>
  <add name="MiniFinansDb"
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=MiniFinansDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
6. Run the project

Press F5

📌 Important Notes
Ensure SQL Server is running
Update connection string before running
Do not share sensitive credentials
📂 Project Structure
Controllers → MVC Controllers
Models → Entity Framework Models
Views → Razor Views
Database → SQL Scripts
Screenshots → Images & GIF
👨‍💻 Developer

Mertcan Kayırıcı

Backend-focused Full Stack Developer
ASP.NET MVC & SQL Server
⭐ Project Purpose

This project was developed to simulate a real-world financial tracking system, focusing on:

Clean architecture
Data visualization
User-friendly dashboard design
