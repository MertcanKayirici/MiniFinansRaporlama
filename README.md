# 💰 Mini Financial Reporting System

A modern and dynamic **financial tracking web application** built with **ASP.NET MVC, Entity Framework, and SQL Server**.

This project enables users to manage income and expense transactions while providing an **interactive dashboard with analytical insights and visual reports**.

---

## 🎬 Demo

> Financial dashboard and transaction management system in action

![Mini Finans Demo](Screenshots/MiniFinansApp.gif)

---

## ✨ Features

- 💸 Income & Expense Management  
- 📊 Real-time Financial Dashboard  
- 📈 Interactive Charts (Chart.js)  
- 🗂️ Category-based Expense Analysis  
- 🔍 Advanced Filtering (Date & Category)  
- 🧾 Transaction Logging System  
- 🎨 Responsive UI (Bootstrap 5)  
- ⚡ Clean & fast user experience  

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

> Relational database structure designed for scalable financial tracking

![ER Diagram](Screenshots/ERDiagram.png)

---

## ⚡ Highlight Feature

The core strength of this project is the **interactive financial dashboard**.

Users can:

- Compare income and expenses instantly  
- Analyze spending by category  
- Track financial trends visually  

👉 This significantly improves **decision-making and user experience**.

---

## ⚙️ Installation

### 1. Clone the repository
```bash
git clone https://github.com/MertcanKayirici/MiniFinansRaporlama.git
```
### 2. Open the project

Open the .sln file using Visual Studio

### 3. Create database

Create a database named:

MiniFinansDB
### 4. Run SQL script

Execute:

Database/MiniFinansRaporlama_DB.sql
### 5. Configure connection string

Update your Web.config:

```bash
<connectionStrings>
  <add name="MiniFinansDb"
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=MiniFinansDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
### 6. Run the project

Press F5 🚀

---

## 📌 Important Notes
- Ensure SQL Server is running
- Update the connection string before running
- Do not share sensitive credentials

---

## 📂 Project Structure
Controllers   → MVC Controllers  
Models        → Entity Framework Models  
Views         → Razor Views  
Database      → SQL Scripts  
Screenshots   → Images & GIF files  

---

## 👨‍💻 Developer

Mertcan Kayırıcı

Backend-focused Full Stack Developer
ASP.NET MVC & SQL Server

---

## ⭐ Project Purpose

This project was developed to simulate a real-world financial tracking system, focusing on:

- Clean architecture principles
- Data visualization techniques
- User-friendly dashboard design

---
