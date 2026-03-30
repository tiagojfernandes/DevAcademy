-- Users
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100)  NOT NULL,
    Email       NVARCHAR(256)  NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    Role        NVARCHAR(20)   NOT NULL DEFAULT 'User',  -- 'User' or 'Admin'
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('User', 'Admin'))
);

-- Products
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
CREATE TABLE Products (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(2000) NULL,
    Price       DECIMAL(18,2)  NOT NULL,
    Stock       INT            NOT NULL DEFAULT 0,
    Category    NVARCHAR(100)  NULL,
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2      NULL,

    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
);

-- Orders
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
CREATE TABLE Orders (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL,
    TotalPrice  DECIMAL(18,2)  NOT NULL,
    Status      NVARCHAR(20)   NOT NULL DEFAULT 'Pending',  -- Pending, Shipped, Delivered, Cancelled
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2      NULL,

    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT CK_Orders_TotalPrice CHECK (TotalPrice >= 0)
);

-- OrderItems
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
CREATE TABLE OrderItems (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    OrderId     INT            NOT NULL,
    ProductId   INT            NOT NULL,
    Quantity    INT            NOT NULL,
    UnitPrice   DECIMAL(18,2)  NOT NULL,

    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0)
);

-- Indexes for performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Category')
    CREATE INDEX IX_Products_Category ON Products(Category);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name')
    CREATE INDEX IX_Products_Name ON Products(Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_UserId')
    CREATE INDEX IX_Orders_UserId ON Orders(UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status')
    CREATE INDEX IX_Orders_Status ON Orders(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedAt')
    CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
    CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_ProductId')
    CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
