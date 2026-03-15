-- Mini Finans Raporlama Veritabanı
-- Bu script projede kullanılan tabloları oluşturur

CREATE TABLE Transactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Date DATETIME NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500),
    Category NVARCHAR(100),
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME
);

CREATE TABLE Logs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Action NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    LogDate DATETIME NOT NULL
);