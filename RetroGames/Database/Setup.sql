
-- RetroGames Database Setup

-- 1. Create Database
IF DB_ID('RetroGamesDB') IS NULL
    CREATE DATABASE RetroGamesDB;
GO

USE RetroGamesDB;
GO

-- 2. Create Categories Table
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Categories
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL
    );
END
GO

-- 3. Insert Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name)
    VALUES
    ('Action'),
    ('Adventure'),
    ('Racing'),
    ('Sports'),
    ('Strategy'),
    ('Puzzle'),
    ('Role-Playing'),
    ('Simulation'),
    ('Horror'),
    ('Fighting');
END
GO

-- 4. Create Products Table
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE Products
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Price DECIMAL(18,2) NOT NULL,
        CategoryId INT NOT NULL,

        CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(Id)
    );
END
GO

-- 5. Insert Sample Products
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products
        (Name, Description, Price, CategoryId)
    VALUES
        ('Grand Theft Auto V', 'Open world action game', 29.99, 1),
        ('Minecraft', 'Creative sandbox game', 24.99, 2),
        ('Forza Horizon 5', 'Open world racing game', 49.99, 3),
        ('FIFA 25', 'Football simulation game', 39.99, 4),
        ('Street Fighter 6', 'Fighting game', 34.99, 10);
END
GO

-- 6. Display Database Records
SELECT * FROM Categories;
SELECT * FROM Products;
GO
