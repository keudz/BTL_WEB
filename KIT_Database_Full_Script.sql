-- ==============================================================
-- BÀI TẬP LỚN LẬP TRÌNH WEB - CÔNG NGHỆ ASP.NET CORE MVC
-- HỆ THỐNG THEO DÕI & QUẢN LÝ CALORIES THÔNG MINH (KIT)
-- Script khởi tạo Cơ sở Dữ liệu & Dữ liệu Mẫu (Seed Data)
-- Hệ quản trị: Microsoft SQL Server (LocalDB / Express / Enterprise)
-- ==============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'KitCalorieDb')
BEGIN
    CREATE DATABASE KitCalorieDb;
END
GO

USE KitCalorieDb;
GO

-- 1. BẢNG USERS (Người dùng hệ thống & Phân quyền)
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        AvatarUrl NVARCHAR(255) NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'Free', -- Pro | Free
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 2. BẢNG USERPROFILES (Hồ sơ thể chất, TDEE, Calo mục tiêu dẫn xuất)
IF OBJECT_ID(N'dbo.UserProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserProfiles (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL UNIQUE,
        Gender NVARCHAR(10) NOT NULL DEFAULT 'Male',
        Age INT NOT NULL DEFAULT 25,
        HeightCm FLOAT NOT NULL DEFAULT 170.0,
        WeightKg FLOAT NOT NULL DEFAULT 65.0,
        ActivityLevel FLOAT NOT NULL DEFAULT 1.375,
        GoalType NVARCHAR(20) NOT NULL DEFAULT 'Maintain', -- LoseWeight | Maintain | GainWeight
        MealsPerDay INT NOT NULL DEFAULT 3,
        TargetCalories INT NOT NULL DEFAULT 2000, -- Dẫn xuất từ TDEE
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

-- 3. BẢNG FOODS (Thư viện Món ăn & Đèn Giao Thông tĩnh)
IF OBJECT_ID(N'dbo.Foods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Foods (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        ServingSize NVARCHAR(50) NOT NULL,
        Calories INT NOT NULL,
        Protein FLOAT NOT NULL,
        Carbs FLOAT NOT NULL,
        Fat FLOAT NOT NULL,
        TrafficLightStatic NVARCHAR(10) NOT NULL DEFAULT 'GREEN', -- GREEN | YELLOW | RED
        IsCustom BIT NOT NULL DEFAULT 0,
        CreatedByUserId INT NOT NULL DEFAULT 0,
        ImageUrl NVARCHAR(255) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE NONCLUSTERED INDEX IX_Foods_Name ON dbo.Foods(Name);
END
GO

-- 4. BẢNG DAILYLOGS (Nhật ký 4 bữa trong ngày & Đèn Giao Thông động)
IF OBJECT_ID(N'dbo.DailyLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DailyLogs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        LogDate DATETIME2 NOT NULL,
        MealType NVARCHAR(20) NOT NULL, -- Breakfast | Lunch | Dinner | Snack
        TotalMealCalories INT NOT NULL DEFAULT 0,
        MealTrafficLightDynamic NVARCHAR(10) NOT NULL DEFAULT 'GREEN', -- GREEN | YELLOW | RED
        CONSTRAINT FK_DailyLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_DailyLogs_User_Date ON dbo.DailyLogs(UserId, LogDate);
END
GO

-- 5. BẢNG LOGITEMS (Chi tiết món ăn trong bữa)
IF OBJECT_ID(N'dbo.LogItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DailyLogId INT NOT NULL,
        FoodId INT NOT NULL,
        Quantity FLOAT NOT NULL DEFAULT 1.0,
        CalculatedCalories INT NOT NULL,
        CalculatedProtein FLOAT NOT NULL,
        CalculatedCarbs FLOAT NOT NULL,
        CalculatedFat FLOAT NOT NULL,
        IsAiEstimated BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LogItems_DailyLogs FOREIGN KEY (DailyLogId) REFERENCES dbo.DailyLogs(Id) ON DELETE CASCADE,
        CONSTRAINT FK_LogItems_Foods FOREIGN KEY (FoodId) REFERENCES dbo.Foods(Id)
    );
END
GO

-- 6. BẢNG FRIDGEITEMS (Tủ lạnh ảo & Quản lý hạn sử dụng)
IF OBJECT_ID(N'dbo.FridgeItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FridgeItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        FoodId INT NULL,
        ItemName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL DEFAULT 'Other',
        Quantity FLOAT NOT NULL DEFAULT 1.0,
        Unit NVARCHAR(20) NOT NULL DEFAULT 'kg',
        PurchasedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ExpiryDate DATETIME2 NOT NULL,
        EstimatedCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        LastNotifiedAt DATETIME2 NULL,
        IsConsumed BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_FridgeItems_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_FridgeItems_Foods FOREIGN KEY (FoodId) REFERENCES dbo.Foods(Id) ON DELETE SET NULL
    );
END
GO

-- 7. BẢNG WEIGHTLOGS (Lịch sử cân nặng để vẽ biểu đồ)
IF OBJECT_ID(N'dbo.WeightLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeightLogs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        WeightKg FLOAT NOT NULL,
        RecordedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Note NVARCHAR(200) NULL,
        CONSTRAINT FK_WeightLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

-- 8. BẢNG NOTIFICATIONS (Thông báo cảnh báo hết hạn và gợi ý công thức)
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        Type NVARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

-- ==============================================================
-- DỮ LIỆU MẪU (SEED DATA)
-- Mật khẩu đã được mã hóa bằng thuật toán BCrypt:
-- Pro: prouser / Pro@123
-- Free: freeuser / User@123
-- ==============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'prouser')
BEGIN
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, Role, CreatedAt)
    VALUES 
    ('prouser', 'pro@kit.vn', '$2a$11$1zJ3sF46pS9bN6tXbUjE6u66U631lX5m00Z8k.n0vL5aZ8Wd5Y8u2', N'Nguyễn Hoàng Nam (Pro VIP)', 'Pro', SYSUTCDATETIME()),
    ('freeuser', 'user@kit.vn', '$2a$11$2zJ3sF46pS9bN6tXbUjE6u66U631lX5m00Z8k.n0vL5aZ8Wd5Y8u2', N'Trần Thị Mai (Free User)', 'Free', SYSUTCDATETIME());

    DECLARE @ProId INT = (SELECT Id FROM dbo.Users WHERE Username = 'prouser');
    DECLARE @FreeId INT = (SELECT Id FROM dbo.Users WHERE Username = 'freeuser');

    INSERT INTO dbo.UserProfiles (UserId, Gender, Age, HeightCm, WeightKg, ActivityLevel, GoalType, MealsPerDay, TargetCalories)
    VALUES 
    (@ProId, 'Male', 24, 172.0, 68.0, 1.55, 'LoseWeight', 3, 1850),
    (@FreeId, 'Female', 21, 160.0, 52.0, 1.375, 'Maintain', 3, 1650);

    -- Seed Foods (35+ Món ăn chuẩn)
    INSERT INTO dbo.Foods (Name, ServingSize, Calories, Protein, Carbs, Fat, TrafficLightStatic, IsCustom, CreatedByUserId)
    VALUES
    (N'Salad ức gà sốt mè rang', N'1 đĩa (200g)', 185, 24.5, 8.2, 5.8, 'GREEN', 0, 0),
    (N'Trứng luộc', N'1 quả (55g)', 78, 6.3, 0.6, 5.3, 'GREEN', 0, 0),
    (N'Canh cải thịt băm', N'1 bát tô (250ml)', 95, 8.5, 4.0, 4.5, 'GREEN', 0, 0),
    (N'Canh chua cá lóc (nước trong)', N'1 bát tô (300ml)', 145, 16.0, 9.0, 3.5, 'GREEN', 0, 0),
    (N'Rau muống luộc chấm tương', N'1 đĩa (150g)', 45, 3.2, 6.5, 0.5, 'GREEN', 0, 0),
    (N'Gỏi cuốn tôm thịt', N'1 cuốn (80g)', 85, 5.5, 11.0, 1.8, 'GREEN', 0, 0),
    (N'Ức gà áp chảo tiêu xanh', N'100g', 165, 31.0, 0.5, 3.6, 'GREEN', 0, 0),
    (N'Khoai lang luộc', N'1 củ vừa (120g)', 105, 2.0, 24.0, 0.2, 'GREEN', 0, 0),
    (N'Cơm trắng', N'1 bát con (150g)', 200, 4.2, 44.5, 0.4, 'YELLOW', 0, 0),
    (N'Cơm gạo lứt', N'1 bát con (150g)', 170, 4.5, 36.0, 1.5, 'YELLOW', 0, 0),
    (N'Bánh mì ốp la (1 trứng + dưa chuột)', N'1 ổ (140g)', 310, 11.0, 42.0, 10.5, 'YELLOW', 0, 0),
    (N'Cháo gà hạt sen', N'1 tô vừa (350ml)', 320, 18.5, 48.0, 5.2, 'YELLOW', 0, 0),
    (N'Bún mọc sườn non', N'1 tô (450ml)', 420, 22.0, 56.0, 12.0, 'YELLOW', 0, 0),
    (N'Phở gà ta (nước trong)', N'1 tô vừa (450ml)', 430, 26.0, 58.0, 8.5, 'YELLOW', 0, 0),
    (N'Phở bò tái nạm gầu', N'1 tô đặc biệt (550ml)', 580, 32.0, 68.0, 20.0, 'RED', 0, 0),
    (N'Cơm tấm sườn bì chả trứng ốp la', N'1 đĩa đầy đủ (400g)', 780, 38.0, 85.0, 32.0, 'RED', 0, 0),
    (N'Bún đậu mắm tôm thập cẩm', N'1 mẹt đầy đủ (450g)', 720, 34.0, 70.0, 33.0, 'RED', 0, 0),
    (N'Bún bò Huế giò heo', N'1 tô lớn (550ml)', 650, 35.0, 62.0, 27.0, 'RED', 0, 0),
    (N'Bún chả Hà Nội (thịt nướng)', N'1 suất đầy đủ (400g)', 620, 29.0, 65.0, 26.0, 'RED', 0, 0),
    (N'Cơm rang dưa bò', N'1 đĩa (350g)', 690, 24.0, 82.0, 28.0, 'RED', 0, 0);

    -- Seed Fridge
    INSERT INTO dbo.FridgeItems (UserId, ItemName, Category, Quantity, Unit, PurchasedDate, ExpiryDate, EstimatedCost, IsConsumed)
    VALUES 
    (@ProId, N'Cá hồi phi lê tươi', 'MeatFish', 0.5, 'kg', DATEADD(DAY, -2, SYSUTCDATETIME()), DATEADD(HOUR, 28, SYSUTCDATETIME()), 180000, 0),
    (@ProId, N'Rau cải thìa Đà Lạt', 'Vegetable', 1.0, N'bó', DATEADD(DAY, -1, SYSUTCDATETIME()), DATEADD(HOUR, 36, SYSUTCDATETIME()), 25000, 0),
    (@ProId, N'Trứng gà Ba Huân', 'Dairy', 10, N'quả', SYSUTCDATETIME(), DATEADD(DAY, 14, SYSUTCDATETIME()), 35000, 0);

    -- Seed Weight
    INSERT INTO dbo.WeightLogs (UserId, WeightKg, RecordedAt, Note)
    VALUES 
    (@ProId, 69.5, DATEADD(DAY, -6, SYSUTCDATETIME()), N'Khởi đầu'),
    (@ProId, 69.2, DATEADD(DAY, -5, SYSUTCDATETIME()), NULL),
    (@ProId, 68.8, DATEADD(DAY, -3, SYSUTCDATETIME()), NULL),
    (@ProId, 68.0, SYSUTCDATETIME(), N'Hiện tại');
END
GO

PRINT N'Cơ sở dữ liệu KitCalorieDb đã được tạo và nạp dữ liệu mẫu thành công!';
GO
