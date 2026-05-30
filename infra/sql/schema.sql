-- ============================================================
-- OnlineStore schema
-- Applies to the database the sqlcmd -d flag points to.
-- Database name is OnlineStoreDb, however locally must be created manually
-- ============================================================

-- Roles
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roles')
CREATE TABLE Roles (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,

    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);

-- Users
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100)  NOT NULL,
    Password     NVARCHAR(512)  NOT NULL,
    LastModified DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted    BIT            NOT NULL DEFAULT 0,

    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);

-- UserRoles
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserRoles')
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Categories
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
CREATE TABLE Categories (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,

    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);

-- Products
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
CREATE TABLE Products (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200)  NOT NULL,
    Description   NVARCHAR(1000) NULL,
    Price         DECIMAL(18,2)  NOT NULL,
    SKU           NVARCHAR(50)   NOT NULL,
    StockQuantity INT            NOT NULL DEFAULT 0,
    CategoryId    INT            NOT NULL,
    IsDeleted     BIT            NOT NULL DEFAULT 0,
    LastModified  DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);

-- ShoppingCart
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ShoppingCart')
CREATE TABLE ShoppingCart (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_ShoppingCart_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- ShoppingCartProducts
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ShoppingCartProducts')
CREATE TABLE ShoppingCartProducts (
    ShoppingCartId INT NOT NULL,
    ProductId      INT NOT NULL,
    Quantity       INT NOT NULL,
    UnitPrice      DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_ShoppingCartProducts PRIMARY KEY (ShoppingCartId, ProductId),
    CONSTRAINT FK_SCP_ShoppingCart FOREIGN KEY (ShoppingCartId) REFERENCES ShoppingCart(Id),
    CONSTRAINT FK_SCP_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_SCP_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SCP_UnitPrice CHECK (UnitPrice >= 0)
);

-- Orders (rebuilt to match FY26 Dev Academy spec: per-user, with OrderItems)
-- NOTE: this DROP wipes any existing Orders data. Dev only.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
    DROP TABLE OrderItems;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
    DROP TABLE Orders;

CREATE TABLE Orders (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    UserId     INT            NOT NULL,
    Status     NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
    TotalPrice DECIMAL(18,2)  NOT NULL,
    CreatedAt  DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT CK_Orders_TotalPrice CHECK (TotalPrice >= 0)
);

CREATE TABLE OrderItems (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    OrderId   INT            NOT NULL,
    ProductId INT            NOT NULL,
    UnitPrice DECIMAL(18,2)  NOT NULL,
    Quantity  INT            NOT NULL,
    LineTotal AS (UnitPrice * Quantity) PERSISTED,

    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_OrderItems_Quantity  CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0)
);

-- Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE INDEX IX_Users_Username ON Users(Username);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_IsDeleted')
    CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CategoryId')
    CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_SKU')
    CREATE INDEX IX_Products_SKU ON Products(SKU);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ShoppingCart_UserId')
    CREATE INDEX IX_ShoppingCart_UserId ON ShoppingCart(UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_UserId')
    CREATE INDEX IX_Orders_UserId ON Orders(UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedAt')
    CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status')
    CREATE INDEX IX_Orders_Status ON Orders(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
    CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_ProductId')
    CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsDeleted')
    CREATE INDEX IX_Products_IsDeleted ON Products(IsDeleted);

-- ------------------------------------------------------------
-- Default roles
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'User')
    INSERT INTO Roles (Name) VALUES ('User');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
    INSERT INTO Roles (Name) VALUES ('Admin');

-- ------------------------------------------------------------
--   Username: admin
--   Password: xxxxxxxxx
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
    INSERT INTO Users (Username, Password)
    VALUES ('admin', 'AQAAAAEAAYagAAAAEGMWmiAvz5x1hDyjEzX1SL/sUc8PRdQeUWiadoGFXk7xR4Jtc75KooH7D4BbiN/wEA==');

IF NOT EXISTS (
    SELECT 1 FROM UserRoles ur
    INNER JOIN Users u ON u.Id = ur.UserId
    INNER JOIN Roles r ON r.Id = ur.RoleId
    WHERE u.Username = 'admin' AND r.Name = 'Admin'
)
    INSERT INTO UserRoles (UserId, RoleId)
    SELECT u.Id, r.Id
    FROM Users u CROSS JOIN Roles r
    WHERE u.Username = 'admin' AND r.Name = 'Admin';

-- ============================================================
-- Views (read-only by admin/reporting endpoints)
-- ============================================================
GO

CREATE OR ALTER VIEW dbo.vw_LowStock AS
SELECT
    p.Id,
    p.Name,
    p.SKU,
    p.StockQuantity,
    c.Name AS CategoryName
FROM dbo.Products p
INNER JOIN dbo.Categories c ON c.Id = p.CategoryId
WHERE p.IsDeleted = 0
  AND p.StockQuantity <= 5;
GO

CREATE OR ALTER VIEW dbo.vw_SalesDaily AS
SELECT
    CAST(o.CreatedAt AS DATE) AS SalesDate,
    COUNT(DISTINCT o.Id)      AS OrderCount,
    SUM(oi.LineTotal)         AS Revenue,
    SUM(oi.Quantity)          AS UnitsSold
FROM dbo.Orders o
INNER JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
WHERE o.Status <> 'Cancelled'
GROUP BY CAST(o.CreatedAt AS DATE);
GO
