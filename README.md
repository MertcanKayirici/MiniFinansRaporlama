# 💰 Mini Financial Reporting System

A modern and dynamic **financial tracking web application** built with **ASP.NET MVC, Entity Framework, and SQL Server**.

This project enables users to manage income and expense transactions while providing an **interactive dashboard with analytical insights, export features, and audit logging system**.

---

## 🎬 Demo GIFs

| 📊 Dashboard Overview |
|----------------------|
| ![](Screenshots/MiniFinans_Dashboard.gif) |

| ➕ Add Transaction | 🔍 Filter Flow |
|-------------------|----------------|
| ![](Screenshots/MiniFinans_AddTransaction.gif) | ![](Screenshots/MiniFinans_Filter.gif) |

---

## ✨ Features

### 💸 Financial Management
- Income & Expense tracking  
- Full CRUD operations (Create, Edit, Details, Delete)  
- Category-based transaction system  
- Date-based filtering (start / end date)  

---

### 📊 Dashboard & Analytics
- Real-time financial summary cards  
- Total income, total expense, net balance  
- Category-based expense distribution  
- Interactive charts using **Chart.js**  
- Monthly financial visualization  

---

### 📁 Export Features (NEW 🚀)
- Export transactions to **Excel**  
- Export transactions to **PDF**  
- Filter-based export (date & category)  
- Clean formatted report output  

---

### 📝 Audit Log System (NEW 🚀)
- Logs all actions (Create, Update, Delete)  
- Stores timestamp and descriptions  
- Tracks user activity flow  
- Improves traceability and system transparency  

---

### 🎨 UI / UX Improvements (NEW 🚀)
- Fully redesigned **Edit / Create / Details pages**  
- Modern card-based UI  
- Improved form usability  
- SweetAlert confirmation system  
- Clean and responsive layout  

---

## 🛠️ Tech Stack

- ASP.NET MVC (.NET Framework)  
- Entity Framework  
- Microsoft SQL Server  
- Bootstrap 5  
- Chart.js  
- SweetAlert2  
- iText7 (PDF Export)  
- ClosedXML (Excel Export)  
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

![](Screenshots/ERDiagram.png)

---

## 🚀 Key Highlights

- Dynamic financial dashboard with real-time analytics  
- Export system (Excel & PDF)  
- Audit logging for all operations  
- Advanced filtering (date & category based)  
- Clean UI/UX with modern design  
- Scalable and maintainable architecture  

---

## 🏗️ Architecture

This project follows a **layered MVC architecture**:

### 🧩 Application Layers
- Controllers → Handle HTTP requests and application flow  
- Models → Represent database entities (Entity Framework)  
- Views → Razor-based UI rendering  

### 🗄️ Data Layer
- Entity Framework for ORM and data access  
- Structured relational database (SQL Server)  

---

## 🔄 How It Works

### 🌐 User Flow
1. User interacts with the dashboard  
2. Data is retrieved from SQL Server via Entity Framework  
3. Controllers process business logic  
4. Views display dynamic financial data  
5. Logs are recorded for each action  

---

## ⚙️ Installation

### 1. Clone the repository
```bash
git clone https://github.com/MertcanKayirici/MiniFinansRaporlama.git
```
### 2. Open the project

Open the `.sln` file using Visual Studio

### 3. Create database

Create a database named:
```plain
MiniFinansDB
```
### 4. Run SQL script

Execute:
```bash
Database/MiniFinansRaporlama_DB.sql
```
### 5. Configure connection string

Update your Web.config:
```xml
<connectionStrings>
  <add name="MiniFinansDb"
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=MiniFinansDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
> ⚠️ Make sure to replace `YOUR_SERVER_NAME` with your SQL Server instance name.

### 6. Run the project

Run the project using **Visual Studio (F5)** 🚀

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
- Data visualization 
- Scalable backend design
- Modern UI/UX experience 

---
