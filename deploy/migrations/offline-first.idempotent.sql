IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Deals] (
        [Id] int NOT NULL IDENTITY,
        [DealName] nvarchar(max) NOT NULL,
        [OriginalPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [FinalPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Deals] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MobileNumber] nvarchar(max) NOT NULL,
        [NationalId] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [SalaryType] int NOT NULL,
        [SalaryAmount] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [KitchenOuts] (
        [Id] int NOT NULL IDENTITY,
        [IssuedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_KitchenOuts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [RawItems] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Unit] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_RawItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [UtilityBills] (
        [Id] int NOT NULL IDENTITY,
        [BillType] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [BillDate] date NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UtilityBills] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Vendors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [WasteRecords] (
        [Id] int NOT NULL IDENTITY,
        [WasteDate] datetime2 NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_WasteRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [MenuItems] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Price] decimal(12,2) NOT NULL,
        [CategoryId] int NOT NULL,
        CONSTRAINT [PK_MenuItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuItems_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [TotalAmount] decimal(12,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [Paid] bit NOT NULL,
        [Status] int NOT NULL,
        [TakenByEmployeeId] int NULL,
        [CashierId] int NULL,
        [CancelledAt] datetime2 NULL,
        [CancelledByEmployeeId] int NULL,
        [PaidAt] datetime2 NULL,
        [PaymentMethod] nvarchar(max) NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Employees_CashierId] FOREIGN KEY ([CashierId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_Employees_TakenByEmployeeId] FOREIGN KEY ([TakenByEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [SalaryPayments] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [SalaryType] int NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [ForDate] date NULL,
        [ForMonth] nvarchar(max) NULL,
        [PaidAt] datetime2 NOT NULL,
        [Remarks] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SalaryPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalaryPayments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [UserName] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [EmployeeId] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [KitchenOutItems] (
        [Id] int NOT NULL IDENTITY,
        [KitchenOutId] int NOT NULL,
        [RawItemId] int NOT NULL,
        [Quantity] decimal(12,4) NOT NULL,
        CONSTRAINT [PK_KitchenOutItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KitchenOutItems_KitchenOuts_KitchenOutId] FOREIGN KEY ([KitchenOutId]) REFERENCES [KitchenOuts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_KitchenOutItems_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [PurchaseOrders] (
        [Id] int NOT NULL IDENTITY,
        [BillNo] nvarchar(100) NOT NULL,
        [PurchaseDate] datetime2 NOT NULL,
        [VendorId] int NOT NULL,
        [TotalAmount] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrders_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [StoreStocks] (
        [Id] int NOT NULL IDENTITY,
        [RawItemId] int NOT NULL,
        [VendorId] int NOT NULL,
        [Quantity] decimal(12,2) NOT NULL,
        [LastUpdated] datetime2 NOT NULL,
        CONSTRAINT [PK_StoreStocks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StoreStocks_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StoreStocks_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [VendorPayments] (
        [Id] int NOT NULL IDENTITY,
        [VendorId] int NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [PaidByUser] nvarchar(max) NOT NULL,
        [PaidOn] datetime2 NOT NULL,
        CONSTRAINT [PK_VendorPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorPayments_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [WasteItems] (
        [Id] int NOT NULL IDENTITY,
        [WasteRecordId] int NOT NULL,
        [RawItemId] int NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_WasteItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WasteItems_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WasteItems_WasteRecords_WasteRecordId] FOREIGN KEY ([WasteRecordId]) REFERENCES [WasteRecords] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [DealItems] (
        [Id] int NOT NULL IDENTITY,
        [DealId] int NOT NULL,
        [MenuItemId] int NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_DealItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DealItems_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [Deals] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DealItems_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [MenuRecipes] (
        [Id] int NOT NULL IDENTITY,
        [MenuItemId] int NOT NULL,
        [RawItemId] int NOT NULL,
        [QuantityRequired] decimal(12,4) NOT NULL,
        CONSTRAINT [PK_MenuRecipes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuRecipes_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MenuRecipes_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [OrderDeals] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [DealId] int NOT NULL,
        [Quantity] int NOT NULL,
        [DealPrice] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_OrderDeals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDeals_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [Deals] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDeals_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [MenuItemId] int NOT NULL,
        [Quantity] decimal(12,2) NOT NULL,
        [UnitPrice] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE TABLE [PurchaseOrderItems] (
        [Id] int NOT NULL IDENTITY,
        [PurchaseOrderId] int NOT NULL,
        [RawItemId] int NOT NULL,
        [UnitPrice] decimal(12,2) NOT NULL,
        [Quantity] decimal(12,2) NOT NULL,
        [TotalPrice] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_PurchaseOrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PurchaseOrderItems_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Name', N'Phone') AND [object_id] = OBJECT_ID(N'[Vendors]'))
        SET IDENTITY_INSERT [Vendors] ON;
    EXEC(N'INSERT INTO [Vendors] ([Id], [Address], [Name], [Phone])
    VALUES (1, N''WahCantt'', N''Big Bull Enterprisers'', N''03035184773''),
    (2, N''RWP'', N''Taj Disposables'', N''03005100401''),
    (3, N''ISB'', N''Jannat Chicken Shop'', N''03175101145''),
    (4, N''WahCantt'', N''Shafqat Vegs'', N''03044545321'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Name', N'Phone') AND [object_id] = OBJECT_ID(N'[Vendors]'))
        SET IDENTITY_INSERT [Vendors] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_DealItems_DealId] ON [DealItems] ([DealId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_DealItems_MenuItemId] ON [DealItems] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_KitchenOutItems_KitchenOutId] ON [KitchenOutItems] ([KitchenOutId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_KitchenOutItems_RawItemId] ON [KitchenOutItems] ([RawItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_MenuItems_CategoryId] ON [MenuItems] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_MenuRecipes_MenuItemId] ON [MenuRecipes] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_MenuRecipes_RawItemId] ON [MenuRecipes] ([RawItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_OrderDeals_DealId] ON [OrderDeals] ([DealId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_OrderDeals_OrderId] ON [OrderDeals] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_OrderItems_MenuItemId] ON [OrderItems] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_Orders_CashierId] ON [Orders] ([CashierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_Orders_TakenByEmployeeId] ON [Orders] ([TakenByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrderItems_PurchaseOrderId] ON [PurchaseOrderItems] ([PurchaseOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrderItems_RawItemId] ON [PurchaseOrderItems] ([RawItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_VendorId] ON [PurchaseOrders] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_SalaryPayments_EmployeeId] ON [SalaryPayments] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoreStocks_RawItemId_VendorId] ON [StoreStocks] ([RawItemId], [VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_StoreStocks_VendorId] ON [StoreStocks] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_Users_EmployeeId] ON [Users] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_VendorPayments_VendorId] ON [VendorPayments] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_WasteItems_RawItemId] ON [WasteItems] ([RawItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    CREATE INDEX [IX_WasteItems_WasteRecordId] ON [WasteItems] ([WasteRecordId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124070801_v1.1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260124070801_v1.1', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124084457_v1.2'
)
BEGIN
    ALTER TABLE [Employees] ADD [Designation] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260124084457_v1.2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260124084457_v1.2', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328092914_AddOnlineOrderFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328092914_AddOnlineOrderFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [CustomerName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328092914_AddOnlineOrderFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [OrderSource] nvarchar(max) NOT NULL DEFAULT N'POS';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328092914_AddOnlineOrderFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [PhoneNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328092914_AddOnlineOrderFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260328092914_AddOnlineOrderFields', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328144001_AddPublicFields'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328144001_AddPublicFields'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [IsAvailable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328144001_AddPublicFields'
)
BEGIN
    ALTER TABLE [Deals] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328144001_AddPublicFields'
)
BEGIN
    ALTER TABLE [Categories] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260328144001_AddPublicFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260328144001_AddPublicFields', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730070629_AddCategoryImageUrl'
)
BEGIN
    ALTER TABLE [Categories] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730070629_AddCategoryImageUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730070629_AddCategoryImageUrl', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730112831_AddMenuItemDescription'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730112831_AddMenuItemDescription'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730112831_AddMenuItemDescription', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730130133_AddDealImageAndOrderServiceType'
)
BEGIN
    ALTER TABLE [Orders] ADD [ServiceType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730130133_AddDealImageAndOrderServiceType'
)
BEGIN
    ALTER TABLE [Deals] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730130133_AddDealImageAndOrderServiceType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730130133_AddDealImageAndOrderServiceType', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    ALTER TABLE [Orders] ADD [Latitude] decimal(9,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    ALTER TABLE [Orders] ADD [Longitude] decimal(9,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    CREATE TABLE [ServiceTimeSettings] (
        [Id] int NOT NULL IDENTITY,
        [ServiceType] nvarchar(20) NOT NULL,
        [MinMinutes] int NOT NULL,
        [MaxMinutes] int NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ServiceTimeSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'MaxMinutes', N'MinMinutes', N'ServiceType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ServiceTimeSettings]'))
        SET IDENTITY_INSERT [ServiceTimeSettings] ON;
    EXEC(N'INSERT INTO [ServiceTimeSettings] ([Id], [MaxMinutes], [MinMinutes], [ServiceType], [UpdatedAt])
    VALUES (1, 20, 15, N''DineIn'', ''2026-01-01T00:00:00.0000000Z''),
    (2, 20, 15, N''Takeaway'', ''2026-01-01T00:00:00.0000000Z''),
    (3, 35, 25, N''Delivery'', ''2026-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'MaxMinutes', N'MinMinutes', N'ServiceType', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ServiceTimeSettings]'))
        SET IDENTITY_INSERT [ServiceTimeSettings] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceTimeSettings_ServiceType] ON [ServiceTimeSettings] ([ServiceType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731160319_AddOrderLocationAndServiceTimeSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731160319_AddOrderLocationAndServiceTimeSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731175844_AddSiteSettings'
)
BEGIN
    CREATE TABLE [SiteSettings] (
        [Id] int NOT NULL IDENTITY,
        [HeroImageUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_SiteSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731175844_AddSiteSettings'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'HeroImageUrl') AND [object_id] = OBJECT_ID(N'[SiteSettings]'))
        SET IDENTITY_INSERT [SiteSettings] ON;
    EXEC(N'INSERT INTO [SiteSettings] ([Id], [HeroImageUrl])
    VALUES (1, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'HeroImageUrl') AND [object_id] = OBJECT_ID(N'[SiteSettings]'))
        SET IDENTITY_INSERT [SiteSettings] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731175844_AddSiteSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731175844_AddSiteSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    ALTER TABLE [Users] ADD [RiderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [RiderCost] decimal(12,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [RiderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    CREATE TABLE [Riders] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [VehicleNumber] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Riders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    CREATE INDEX [IX_Users_RiderId] ON [Users] ([RiderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    CREATE INDEX [IX_Orders_RiderId] ON [Orders] ([RiderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Riders_RiderId] FOREIGN KEY ([RiderId]) REFERENCES [Riders] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Riders_RiderId] FOREIGN KEY ([RiderId]) REFERENCES [Riders] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801075448_AddRiderManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801075448_AddRiderManagement', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    ALTER TABLE [Orders] ADD [AreaId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryFeeCharged] decimal(12,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryStatus] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    CREATE TABLE [Areas] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [DeliveryFee] decimal(12,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Areas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DeliveryFee', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] ON;
    EXEC(N'INSERT INTO [Areas] ([Id], [CreatedAt], [DeliveryFee], [IsActive], [Name])
    VALUES (1, ''2026-01-01T00:00:00.0000000Z'', 100.0, CAST(1 AS bit), N''Unknown'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DeliveryFee', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    CREATE INDEX [IX_Orders_AreaId] ON [Orders] ([AreaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Areas_Name] ON [Areas] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Areas_AreaId] FOREIGN KEY ([AreaId]) REFERENCES [Areas] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807175338_AddAreasAndDeliveryStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807175338_AddAreasAndDeliveryStatus', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [WhatsAppNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [IsEnabled] = CAST(1 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [IsEnabled] = CAST(1 AS bit)
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [IsEnabled] = CAST(1 AS bit)
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [WhatsAppNumber] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808155531_AddWhatsAppNumberAndServiceTypeEnabled', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811142226_AddOrderCashierAndRejectReasonFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [CancelledByUserName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811142226_AddOrderCashierAndRejectReasonFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [CashierUserName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811142226_AddOrderCashierAndRejectReasonFields'
)
BEGIN
    ALTER TABLE [Orders] ADD [RejectReason] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811142226_AddOrderCashierAndRejectReasonFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811142226_AddOrderCashierAndRejectReasonFields', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811151912_AddSiteSettingBranding'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [LogoUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811151912_AddSiteSettingBranding'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [RestaurantName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811151912_AddSiteSettingBranding'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [LogoUrl] = NULL, [RestaurantName] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811151912_AddSiteSettingBranding'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811151912_AddSiteSettingBranding', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    ALTER TABLE [Vendors] ADD [LeadTimeDays] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    ALTER TABLE [Vendors] ADD [MinimumOrderQuantity] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    ALTER TABLE [RawItems] ADD [SafetyStockQuantity] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    ALTER TABLE [RawItems] ADD [ShelfLifeDays] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiAuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [Feature] nvarchar(450) NOT NULL,
        [UserId] int NULL,
        [UserName] nvarchar(max) NULL,
        [Role] nvarchar(max) NULL,
        [RequestSummary] nvarchar(max) NULL,
        [ResponseSummary] nvarchar(max) NULL,
        [Provider] nvarchar(max) NULL,
        [Model] nvarchar(max) NULL,
        [InputTokens] int NULL,
        [OutputTokens] int NULL,
        [Success] bit NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [DurationMs] bigint NOT NULL,
        CONSTRAINT [PK_AiAuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiConversations] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [LastMessageAt] datetime2 NOT NULL,
        [UserId] int NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NULL,
        CONSTRAINT [PK_AiConversations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiForecastRuns] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [ModelVersion] nvarchar(max) NOT NULL,
        [ForecastFrom] date NOT NULL,
        [ForecastTo] date NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Mae] decimal(18,2) NULL,
        [Wape] decimal(18,2) NULL,
        CONSTRAINT [PK_AiForecastRuns] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiInventoryRecommendations] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [RawItemId] int NOT NULL,
        [CurrentStock] decimal(18,2) NOT NULL,
        [ForecastedDemand] decimal(18,2) NOT NULL,
        [SuggestedReorderQuantity] decimal(18,2) NOT NULL,
        [SuggestedReorderDate] date NULL,
        [RecommendationType] nvarchar(max) NOT NULL,
        [Explanation] nvarchar(max) NOT NULL,
        [DataWarnings] nvarchar(max) NULL,
        [ConfidenceLow] decimal(18,2) NOT NULL,
        [ConfidenceHigh] decimal(18,2) NOT NULL,
        [Status] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AiInventoryRecommendations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiInventoryRecommendations_RawItems_RawItemId] FOREIGN KEY ([RawItemId]) REFERENCES [RawItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiToolExecutionRecords] (
        [Id] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ToolName] nvarchar(max) NOT NULL,
        [ArgumentsJson] nvarchar(max) NOT NULL,
        [Success] bit NOT NULL,
        [ResultSummary] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [DurationMs] bigint NOT NULL,
        CONSTRAINT [PK_AiToolExecutionRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [PhoneNumber] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NULL,
        [PersonalizationConsent] bit NOT NULL,
        [Allergens] nvarchar(max) NULL,
        [DietaryPreferences] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiMessageRecords] (
        [Id] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AiMessageRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiMessageRecords_AiConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [AiConversations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiForecastValues] (
        [Id] int NOT NULL IDENTITY,
        [ForecastRunId] int NOT NULL,
        [ForecastDate] date NOT NULL,
        [HourOfDay] int NULL,
        [MenuItemId] int NULL,
        [PredictedSales] decimal(18,2) NOT NULL,
        [PredictedOrderCount] int NOT NULL,
        [PredictedQuantity] decimal(18,2) NOT NULL,
        [ConfidenceLow] decimal(18,2) NOT NULL,
        [ConfidenceHigh] decimal(18,2) NOT NULL,
        [LowConfidence] bit NOT NULL,
        [ActualSales] decimal(18,2) NULL,
        [ActualOrderCount] int NULL,
        [ActualQuantity] decimal(18,2) NULL,
        CONSTRAINT [PK_AiForecastValues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiForecastValues_AiForecastRuns_ForecastRunId] FOREIGN KEY ([ForecastRunId]) REFERENCES [AiForecastRuns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AiForecastValues_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE TABLE [AiInventoryRecommendationDecisions] (
        [Id] int NOT NULL IDENTITY,
        [RecommendationId] int NOT NULL,
        [DecidedAt] datetime2 NOT NULL,
        [DecidedByUserId] int NULL,
        [DecidedByUserName] nvarchar(max) NULL,
        [Decision] nvarchar(max) NOT NULL,
        [ModifiedQuantity] decimal(18,2) NULL,
        [Feedback] nvarchar(max) NULL,
        CONSTRAINT [PK_AiInventoryRecommendationDecisions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiInventoryRecommendationDecisions_AiInventoryRecommendations_RecommendationId] FOREIGN KEY ([RecommendationId]) REFERENCES [AiInventoryRecommendations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiAuditLogs_CreatedAt] ON [AiAuditLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiAuditLogs_Feature] ON [AiAuditLogs] ([Feature]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiConversations_UserId] ON [AiConversations] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiForecastRuns_ForecastFrom_ForecastTo] ON [AiForecastRuns] ([ForecastFrom], [ForecastTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiForecastValues_ForecastDate_MenuItemId_HourOfDay] ON [AiForecastValues] ([ForecastDate], [MenuItemId], [HourOfDay]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiForecastValues_ForecastRunId] ON [AiForecastValues] ([ForecastRunId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiForecastValues_MenuItemId] ON [AiForecastValues] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiInventoryRecommendationDecisions_RecommendationId] ON [AiInventoryRecommendationDecisions] ([RecommendationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiInventoryRecommendations_CreatedAt] ON [AiInventoryRecommendations] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiInventoryRecommendations_RawItemId_Status] ON [AiInventoryRecommendations] ([RawItemId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiMessageRecords_ConversationId] ON [AiMessageRecords] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE INDEX [IX_AiToolExecutionRecords_ConversationId] ON [AiToolExecutionRecords] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_PhoneNumber] ON [Customers] ([PhoneNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811180234_AddAiFoundationAndCustomer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811180234_AddAiFoundationAndCustomer', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OrderSerialCurrentDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OrderSerialCurrentNumber] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OrderSerialPrefix] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OrderSerialResetTime] time NOT NULL DEFAULT '00:00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OrderSerialStartingNumber] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    ALTER TABLE [Orders] ADD [OrderNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [OrderSerialCurrentDate] = NULL, [OrderSerialCurrentNumber] = 0, [OrderSerialPrefix] = N'''', [OrderSerialResetTime] = ''00:00:00'', [OrderSerialStartingNumber] = 1
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812095632_AddOrderSerialSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812095632_AddOrderSerialSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812101706_AddMenuPdf'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [MenuPdfUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812101706_AddMenuPdf'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [MenuPdfUrl] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812101706_AddMenuPdf'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812101706_AddMenuPdf', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812122553_AddCountryAndTimeZoneToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [Country] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812122553_AddCountryAndTimeZoneToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [TimeZoneId] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812122553_AddCountryAndTimeZoneToSiteSettings'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [Country] = N''Pakistan'', [TimeZoneId] = N''Asia/Karachi''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812122553_AddCountryAndTimeZoneToSiteSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812122553_AddCountryAndTimeZoneToSiteSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812124643_AddCurrencyToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [CurrencyCode] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812124643_AddCurrencyToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [CurrencySymbol] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812124643_AddCurrencyToSiteSettings'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [CurrencyCode] = N''PKR'', [CurrencySymbol] = N''Rs''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812124643_AddCurrencyToSiteSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812124643_AddCurrencyToSiteSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813100307_AddAddressToCustomer'
)
BEGIN
    ALTER TABLE [Customers] ADD [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813100307_AddAddressToCustomer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813100307_AddAddressToCustomer', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813125108_AddCompanyBrandingToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [CompanyLogoUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813125108_AddCompanyBrandingToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [CompanyName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813125108_AddCompanyBrandingToSiteSettings'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [CompanyLogoUrl] = NULL, [CompanyName] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813125108_AddCompanyBrandingToSiteSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813125108_AddCompanyBrandingToSiteSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816145148_AddGoogleMapsUrlToSiteSettings'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [GoogleMapsUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816145148_AddGoogleMapsUrlToSiteSettings'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [GoogleMapsUrl] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816145148_AddGoogleMapsUrlToSiteSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260816145148_AddGoogleMapsUrlToSiteSettings', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(32) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE TABLE [NodeHeartbeats] (
        [Id] bigint NOT NULL IDENTITY,
        [NodeId] uniqueidentifier NOT NULL,
        [Role] nvarchar(16) NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [SentAtUtc] datetime2 NOT NULL,
        [ReceivedAtUtc] datetime2 NOT NULL,
        [AppVersion] nvarchar(64) NULL,
        [SchemaVersion] nvarchar(128) NULL,
        [PendingOutboxCount] int NOT NULL,
        [Source] nvarchar(64) NULL,
        [DetailsJson] nvarchar(max) NULL,
        CONSTRAINT [PK_NodeHeartbeats] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE TABLE [SystemNodes] (
        [Id] int NOT NULL IDENTITY,
        [NodeId] uniqueidentifier NOT NULL,
        [Role] nvarchar(16) NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [DisplayName] nvarchar(200) NULL,
        [BaseUrl] nvarchar(512) NULL,
        [AppVersion] nvarchar(64) NULL,
        [SchemaVersion] nvarchar(128) NULL,
        [IsActive] bit NOT NULL,
        [RegisteredAtUtc] datetime2 NOT NULL,
        [LastSeenAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SystemNodes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_BranchId] ON [Branches] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE INDEX [IX_NodeHeartbeats_NodeId_ReceivedAtUtc] ON [NodeHeartbeats] ([NodeId], [ReceivedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE INDEX [IX_SystemNodes_BranchId] ON [SystemNodes] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemNodes_NodeId] ON [SystemNodes] ([NodeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831101207_AddNodeAndBranchIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831101207_AddNodeAndBranchIdentity', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteRecords] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [WasteItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Vendors] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Users] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [SiteSettings] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [ServiceTimeSettings] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Riders] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [RawItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrders] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrderItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrderItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [PurchaseOrderItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Orders] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderDeals] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderDeals] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [OrderDeals] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuRecipes] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuRecipes] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuRecipes] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOuts] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOutItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOutItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [KitchenOutItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Deals] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [DealItems] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [DealItems] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [DealItems] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Customers] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Categories] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [AggregateVersion] bigint NOT NULL DEFAULT (1);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [DeletedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    ALTER TABLE [Areas] ADD [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE TABLE [SyncTombstones] (
        [Id] bigint NOT NULL IDENTITY,
        [GlobalId] uniqueidentifier NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [OriginNodeId] uniqueidentifier NOT NULL,
        [AggregateVersion] bigint NOT NULL,
        [DeletedAtUtc] datetime2 NOT NULL,
        [Dispatched] bit NOT NULL,
        CONSTRAINT [PK_SyncTombstones] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    EXEC(N'UPDATE [Areas] SET [DeletedAtUtc] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [DeletedAtUtc] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [DeletedAtUtc] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    EXEC(N'UPDATE [ServiceTimeSettings] SET [DeletedAtUtc] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    EXEC(N'UPDATE [SiteSettings] SET [DeletedAtUtc] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_WasteRecords_BranchId_UpdatedAtUtc] ON [WasteRecords] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_WasteRecords_DeletedAtUtc] ON [WasteRecords] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WasteRecords_GlobalId] ON [WasteRecords] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WasteItems_GlobalId] ON [WasteItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Vendors_BranchId_UpdatedAtUtc] ON [Vendors] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Vendors_DeletedAtUtc] ON [Vendors] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vendors_GlobalId] ON [Vendors] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Users_BranchId_UpdatedAtUtc] ON [Users] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Users_DeletedAtUtc] ON [Users] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_GlobalId] ON [Users] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_SiteSettings_BranchId_UpdatedAtUtc] ON [SiteSettings] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_SiteSettings_DeletedAtUtc] ON [SiteSettings] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SiteSettings_GlobalId] ON [SiteSettings] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_ServiceTimeSettings_BranchId_UpdatedAtUtc] ON [ServiceTimeSettings] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_ServiceTimeSettings_DeletedAtUtc] ON [ServiceTimeSettings] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceTimeSettings_GlobalId] ON [ServiceTimeSettings] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Riders_BranchId_UpdatedAtUtc] ON [Riders] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Riders_DeletedAtUtc] ON [Riders] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Riders_GlobalId] ON [Riders] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_RawItems_BranchId_UpdatedAtUtc] ON [RawItems] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_RawItems_DeletedAtUtc] ON [RawItems] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RawItems_GlobalId] ON [RawItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_BranchId_UpdatedAtUtc] ON [PurchaseOrders] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_DeletedAtUtc] ON [PurchaseOrders] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseOrders_GlobalId] ON [PurchaseOrders] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseOrderItems_GlobalId] ON [PurchaseOrderItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Orders_BranchId_UpdatedAtUtc] ON [Orders] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Orders_DeletedAtUtc] ON [Orders] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_GlobalId] ON [Orders] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderItems_GlobalId] ON [OrderItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderDeals_GlobalId] ON [OrderDeals] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MenuRecipes_GlobalId] ON [MenuRecipes] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_MenuItems_BranchId_UpdatedAtUtc] ON [MenuItems] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_MenuItems_DeletedAtUtc] ON [MenuItems] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MenuItems_GlobalId] ON [MenuItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_KitchenOuts_BranchId_UpdatedAtUtc] ON [KitchenOuts] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_KitchenOuts_DeletedAtUtc] ON [KitchenOuts] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenOuts_GlobalId] ON [KitchenOuts] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenOutItems_GlobalId] ON [KitchenOutItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Deals_BranchId_UpdatedAtUtc] ON [Deals] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Deals_DeletedAtUtc] ON [Deals] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Deals_GlobalId] ON [Deals] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DealItems_GlobalId] ON [DealItems] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Customers_BranchId_UpdatedAtUtc] ON [Customers] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Customers_DeletedAtUtc] ON [Customers] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_GlobalId] ON [Customers] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Categories_BranchId_UpdatedAtUtc] ON [Categories] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Categories_DeletedAtUtc] ON [Categories] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_GlobalId] ON [Categories] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Areas_BranchId_UpdatedAtUtc] ON [Areas] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_Areas_DeletedAtUtc] ON [Areas] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Areas_GlobalId] ON [Areas] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE INDEX [IX_SyncTombstones_Dispatched_DeletedAtUtc] ON [SyncTombstones] ([Dispatched], [DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SyncTombstones_GlobalId] ON [SyncTombstones] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831102526_AddSyncIdentityColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831102526_AddSyncIdentityColumns', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103145_AddSchemaMigrationHistory'
)
BEGIN
    CREATE TABLE [SchemaMigrationHistories] (
        [Id] int NOT NULL IDENTITY,
        [StartedAtUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        [FromMigration] nvarchar(200) NULL,
        [ToMigration] nvarchar(200) NULL,
        [AppliedCount] int NOT NULL,
        [AppVersion] nvarchar(64) NULL,
        [NodeId] uniqueidentifier NOT NULL,
        [NodeRole] nvarchar(16) NOT NULL,
        [BackupPath] nvarchar(1024) NULL,
        [BackupTaken] bit NOT NULL,
        [Outcome] nvarchar(16) NOT NULL,
        [Error] nvarchar(max) NULL,
        CONSTRAINT [PK_SchemaMigrationHistories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103145_AddSchemaMigrationHistory'
)
BEGIN
    CREATE INDEX [IX_SchemaMigrationHistories_StartedAtUtc] ON [SchemaMigrationHistories] ([StartedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103145_AddSchemaMigrationHistory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831103145_AddSchemaMigrationHistory', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103654_AddProcessedCommands'
)
BEGIN
    CREATE TABLE [ProcessedCommands] (
        [Id] bigint NOT NULL IDENTITY,
        [CommandId] uniqueidentifier NOT NULL,
        [NodeId] uniqueidentifier NOT NULL,
        [Route] nvarchar(400) NOT NULL,
        [RequestHash] nvarchar(64) NOT NULL,
        [State] nvarchar(16) NOT NULL,
        [StatusCode] int NULL,
        [ResultGlobalId] uniqueidentifier NULL,
        [ResponseContentType] nvarchar(128) NULL,
        [ResponseBody] nvarchar(max) NULL,
        [ResponseTruncated] bit NOT NULL,
        [StartedAtUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ProcessedCommands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103654_AddProcessedCommands'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProcessedCommands_CommandId] ON [ProcessedCommands] ([CommandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103654_AddProcessedCommands'
)
BEGIN
    CREATE INDEX [IX_ProcessedCommands_StartedAtUtc] ON [ProcessedCommands] ([StartedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831103654_AddProcessedCommands'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831103654_AddProcessedCommands', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'OrderNumber');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [OrderNumber] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    ALTER TABLE [Orders] ADD [OrderNumberSource] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    CREATE TABLE [OrderNumberSequences] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] uniqueidentifier NOT NULL,
        [SourceCode] nvarchar(8) NOT NULL,
        [BusinessDate] date NOT NULL,
        [LastValue] int NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_OrderNumberSequences] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    CREATE INDEX [IX_Orders_OrderNumberSource] ON [Orders] ([OrderNumberSource]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Orders_OrderNumber_Sequenced] ON [Orders] ([OrderNumber]) WHERE [OrderNumberSource] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderNumberSequences_BranchId_SourceCode_BusinessDate] ON [OrderNumberSequences] ([BranchId], [SourceCode], [BusinessDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831104157_AddOrderNumberSequences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831104157_AddOrderNumberSequences', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE TABLE [StockMovements] (
        [Id] bigint NOT NULL IDENTITY,
        [MovementType] nvarchar(32) NOT NULL,
        [RawItemId] int NOT NULL,
        [RawItemGlobalId] uniqueidentifier NOT NULL,
        [VendorId] int NULL,
        [VendorGlobalId] uniqueidentifier NULL,
        [QuantityDelta] decimal(18,4) NOT NULL,
        [ReferenceType] nvarchar(64) NOT NULL,
        [ReferenceGlobalId] uniqueidentifier NOT NULL,
        [ReversesMovementGlobalId] uniqueidentifier NULL,
        [OccurredAtUtc] datetime2 NOT NULL,
        [CreatedByUserGlobalId] uniqueidentifier NULL,
        [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
        [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
        [AggregateVersion] bigint NOT NULL DEFAULT (1),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE INDEX [IX_StockMovements_BranchId_UpdatedAtUtc] ON [StockMovements] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE INDEX [IX_StockMovements_DeletedAtUtc] ON [StockMovements] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockMovements_GlobalId] ON [StockMovements] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE INDEX [IX_StockMovements_OccurredAtUtc] ON [StockMovements] ([OccurredAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE INDEX [IX_StockMovements_RawItemId_VendorId] ON [StockMovements] ([RawItemId], [VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    CREATE UNIQUE INDEX [UX_StockMovements_Reference] ON [StockMovements] ([ReferenceType], [ReferenceGlobalId], [MovementType], [RawItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831105110_AddStockMovementLedger'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831105110_AddStockMovementLedger', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncCheckpoints] (
        [Id] int NOT NULL IDENTITY,
        [PeerNodeId] uniqueidentifier NOT NULL,
        [Direction] nvarchar(8) NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [LastValue] bigint NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SyncCheckpoints] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncConflicts] (
        [Id] bigint NOT NULL IDENTITY,
        [EventId] uniqueidentifier NOT NULL,
        [Kind] nvarchar(32) NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [AggregateGlobalId] uniqueidentifier NOT NULL,
        [IncomingVersion] bigint NOT NULL,
        [LocalVersion] bigint NOT NULL,
        [OriginNodeId] uniqueidentifier NOT NULL,
        [IncomingPayloadJson] nvarchar(max) NULL,
        [LocalSnapshotJson] nvarchar(max) NULL,
        [Detail] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [Resolved] bit NOT NULL,
        [Resolution] nvarchar(16) NULL,
        [ResolvedByUserName] nvarchar(max) NULL,
        [ResolvedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_SyncConflicts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncDeadLetters] (
        [Id] bigint NOT NULL IDENTITY,
        [EventId] uniqueidentifier NOT NULL,
        [Kind] nvarchar(32) NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [AggregateGlobalId] uniqueidentifier NOT NULL,
        [SchemaVersion] int NOT NULL,
        [EnvelopeJson] nvarchar(max) NOT NULL,
        [Error] nvarchar(max) NULL,
        [Attempts] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [LastAttemptAtUtc] datetime2 NULL,
        [Replayed] bit NOT NULL,
        CONSTRAINT [PK_SyncDeadLetters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncInbox] (
        [Id] bigint NOT NULL IDENTITY,
        [EventId] uniqueidentifier NOT NULL,
        [EventType] nvarchar(128) NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [AggregateGlobalId] uniqueidentifier NOT NULL,
        [AggregateVersion] bigint NOT NULL,
        [OriginNodeId] uniqueidentifier NOT NULL,
        [ReceivedAtUtc] datetime2 NOT NULL,
        [AppliedAtUtc] datetime2 NOT NULL,
        [Status] nvarchar(16) NOT NULL,
        [ConflictId] bigint NULL,
        CONSTRAINT [PK_SyncInbox] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncNonces] (
        [Id] bigint NOT NULL IDENTITY,
        [NodeId] uniqueidentifier NOT NULL,
        [Nonce] nvarchar(64) NOT NULL,
        [SeenAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_SyncNonces] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE TABLE [SyncOutbox] (
        [Id] bigint NOT NULL IDENTITY,
        [EventId] uniqueidentifier NOT NULL,
        [EventType] nvarchar(128) NOT NULL,
        [SchemaVersion] int NOT NULL,
        [AggregateType] nvarchar(128) NOT NULL,
        [AggregateGlobalId] uniqueidentifier NOT NULL,
        [AggregateVersion] bigint NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [OriginNodeId] uniqueidentifier NOT NULL,
        [OccurredAtUtc] datetime2 NOT NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [CausationId] uniqueidentifier NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [Dispatched] bit NOT NULL,
        [DispatchedAtUtc] datetime2 NULL,
        [Attempts] int NOT NULL,
        [LastError] nvarchar(max) NULL,
        CONSTRAINT [PK_SyncOutbox] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SyncCheckpoints_PeerNodeId_Direction_AggregateType] ON [SyncCheckpoints] ([PeerNodeId], [Direction], [AggregateType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncConflicts_EventId] ON [SyncConflicts] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncConflicts_Resolved_CreatedAtUtc] ON [SyncConflicts] ([Resolved], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncDeadLetters_EventId] ON [SyncDeadLetters] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncDeadLetters_Replayed_CreatedAtUtc] ON [SyncDeadLetters] ([Replayed], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncInbox_AggregateType_AggregateGlobalId] ON [SyncInbox] ([AggregateType], [AggregateGlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SyncInbox_EventId] ON [SyncInbox] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SyncNonces_NodeId_Nonce] ON [SyncNonces] ([NodeId], [Nonce]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncNonces_SeenAtUtc] ON [SyncNonces] ([SeenAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE INDEX [IX_SyncOutbox_Dispatched_Id] ON [SyncOutbox] ([Dispatched], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SyncOutbox_EventId] ON [SyncOutbox] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831110632_AddSyncEngine'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831110632_AddSyncEngine', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE TABLE [UploadedFiles] (
        [Id] bigint NOT NULL IDENTITY,
        [StorageKey] nvarchar(512) NOT NULL,
        [OriginalFileName] nvarchar(max) NULL,
        [ContentType] nvarchar(128) NOT NULL,
        [Size] bigint NOT NULL,
        [Sha256Hash] nvarchar(64) NOT NULL,
        [Category] nvarchar(64) NOT NULL,
        [SyncState] nvarchar(16) NOT NULL,
        [GlobalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [BranchId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
        [OriginNodeId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
        [AggregateVersion] bigint NOT NULL DEFAULT (1),
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DeletedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_UploadedFiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE INDEX [IX_UploadedFiles_BranchId_UpdatedAtUtc] ON [UploadedFiles] ([BranchId], [UpdatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE INDEX [IX_UploadedFiles_DeletedAtUtc] ON [UploadedFiles] ([DeletedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UploadedFiles_GlobalId] ON [UploadedFiles] ([GlobalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE INDEX [IX_UploadedFiles_Sha256Hash] ON [UploadedFiles] ([Sha256Hash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UploadedFiles_StorageKey] ON [UploadedFiles] ([StorageKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831111606_AddUploadedFiles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831111606_AddUploadedFiles', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    ALTER TABLE [Users] ADD [SecurityStamp] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    CREATE TABLE [AuthAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [AtUtc] datetime2 NOT NULL,
        [UserName] nvarchar(128) NOT NULL,
        [Role] nvarchar(32) NULL,
        [Result] nvarchar(32) NOT NULL,
        [Issuer] nvarchar(64) NULL,
        [NodeId] uniqueidentifier NOT NULL,
        [IpAddress] nvarchar(64) NULL,
        [Detail] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthAuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    CREATE INDEX [IX_AuthAuditLogs_AtUtc] ON [AuthAuditLogs] ([AtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    CREATE INDEX [IX_AuthAuditLogs_UserName] ON [AuthAuditLogs] ([UserName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112320_AddOfflineAuth'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831112320_AddOfflineAuth', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112655_AddPrintJobs'
)
BEGIN
    CREATE TABLE [PrintJobs] (
        [Id] bigint NOT NULL IDENTITY,
        [PrintJobId] uniqueidentifier NOT NULL,
        [JobType] nvarchar(32) NOT NULL,
        [Copy] nvarchar(32) NOT NULL,
        [OrderId] int NULL,
        [OrderGlobalId] uniqueidentifier NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [Status] nvarchar(16) NOT NULL,
        [Attempts] int NOT NULL,
        [Error] nvarchar(max) NULL,
        [IsReprint] bit NOT NULL,
        [ReprintReason] nvarchar(max) NULL,
        [RequestedByUserName] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PrintJobs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112655_AddPrintJobs'
)
BEGIN
    CREATE INDEX [IX_PrintJobs_OrderGlobalId_JobType_Copy_Status] ON [PrintJobs] ([OrderGlobalId], [JobType], [Copy], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112655_AddPrintJobs'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PrintJobs_PrintJobId] ON [PrintJobs] ([PrintJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260831112655_AddPrintJobs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260831112655_AddPrintJobs', N'8.0.22');
END;
GO

COMMIT;
GO

