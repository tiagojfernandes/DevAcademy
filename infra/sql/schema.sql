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
    Price         DECIMAL(18,2)  NOT NULL,
    SKU           NVARCHAR(50)   NOT NULL,
    StockQuantity INT            NOT NULL DEFAULT 0,
    CategoryId    INT            NOT NULL,
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

    CONSTRAINT PK_ShoppingCartProducts PRIMARY KEY (ShoppingCartId, ProductId),
    CONSTRAINT FK_SCP_ShoppingCart FOREIGN KEY (ShoppingCartId) REFERENCES ShoppingCart(Id),
    CONSTRAINT FK_SCP_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_SCP_Quantity CHECK (Quantity > 0)
);

-- Orders
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
CREATE TABLE Orders (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    ShoppingCartId INT            NOT NULL,
    Timestamp      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    TotalPrice     DECIMAL(18,2)  NOT NULL,
    Status         NVARCHAR(20)   NOT NULL DEFAULT 'Pending',

    CONSTRAINT FK_Orders_ShoppingCart FOREIGN KEY (ShoppingCartId) REFERENCES ShoppingCart(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT CK_Orders_TotalPrice CHECK (TotalPrice >= 0)
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_ShoppingCartId')
    CREATE INDEX IX_Orders_ShoppingCartId ON Orders(ShoppingCartId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status')
    CREATE INDEX IX_Orders_Status ON Orders(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Timestamp')
    CREATE INDEX IX_Orders_Timestamp ON Orders(Timestamp);
