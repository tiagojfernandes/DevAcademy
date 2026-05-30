-- ============================================================
-- seed data for Categories and Products  
-- ============================================================

-- Categories
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Phones')
    INSERT INTO Categories (Name) VALUES (N'Phones');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Laptops')
    INSERT INTO Categories (Name) VALUES (N'Laptops');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Accessories')
    INSERT INTO Categories (Name) VALUES (N'Accessories');

-- Products 
DECLARE @Phones      INT = (SELECT Id FROM Categories WHERE Name = N'Phones');
DECLARE @Laptops     INT = (SELECT Id FROM Categories WHERE Name = N'Laptops');
DECLARE @Accessories INT = (SELECT Id FROM Categories WHERE Name = N'Accessories');

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'PH-PIXEL-9')
    INSERT INTO Products (Name, Description, Price, SKU, StockQuantity, CategoryId)
    VALUES (N'Pixel 9', N'Google flagship', 899.00, N'PH-PIXEL-9', 25, @Phones);

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'LT-MBP-14')
    INSERT INTO Products (Name, Description, Price, SKU, StockQuantity, CategoryId)
    VALUES (N'MacBook Pro 14"', N'M4, 16GB/512GB', 1999.00, N'LT-MBP-14', 8, @Laptops);

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'LT-XPS-13')
    INSERT INTO Products (Name, Description, Price, SKU, StockQuantity, CategoryId)
    VALUES (N'Dell XPS 13', N'Intel Core Ultra 7', 1499.00, N'LT-XPS-13', 12, @Laptops);

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'AC-USBC-CBL')
    INSERT INTO Products (Name, Description, Price, SKU, StockQuantity, CategoryId)
    VALUES (N'USB-C Cable 1m', N'Braided 100W', 12.50, N'AC-USBC-CBL', 200, @Accessories);

GO

SELECT Id, Name FROM Categories ORDER BY Id;
SELECT Id, Name, SKU, Price, StockQuantity, CategoryId FROM Products ORDER BY Id;
