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
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [Admins] (
        [AdminId] int NOT NULL IDENTITY,
        [AdminName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Admins] PRIMARY KEY ([AdminId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [UserName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [Places] (
        [PlaceId] int NOT NULL IDENTITY,
        [OwnerId] int NOT NULL,
        [PlaceName] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Price] int NOT NULL,
        [RangeWork] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Places] PRIMARY KEY ([PlaceId]),
        CONSTRAINT [FK_Places_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [Bookings] (
        [BookingId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [PlaceId] int NOT NULL,
        [TotalPrice] int NOT NULL,
        [BookingDate] datetime2 NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
        CONSTRAINT [FK_Bookings_Places_PlaceId] FOREIGN KEY ([PlaceId]) REFERENCES [Places] ([PlaceId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Bookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [PlaceImages] (
        [ImageId] int NOT NULL IDENTITY,
        [PlaceId] int NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PlaceImages] PRIMARY KEY ([ImageId]),
        CONSTRAINT [FK_PlaceImages_Places_PlaceId] FOREIGN KEY ([PlaceId]) REFERENCES [Places] ([PlaceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE TABLE [Reviews] (
        [ReviewId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [PlaceId] int NOT NULL,
        [Rating] int NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
        CONSTRAINT [FK_Reviews_Places_PlaceId] FOREIGN KEY ([PlaceId]) REFERENCES [Places] ([PlaceId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'Email', N'Location', N'Password', N'PhoneNumber', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([UserId], [Email], [Location], [Password], [PhoneNumber], [UserName])
    VALUES (1, N''khalid@gmail.com'', N''zarqa'', N''1234'', N''0781234567'', N''khalid'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'Email', N'Location', N'Password', N'PhoneNumber', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_Bookings_PlaceId] ON [Bookings] ([PlaceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_Bookings_UserId] ON [Bookings] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_PlaceImages_PlaceId] ON [PlaceImages] ([PlaceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_Places_OwnerId] ON [Places] ([OwnerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_Reviews_PlaceId] ON [Reviews] ([PlaceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028170242_adds'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241028170242_adds', N'8.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028201550_adddddddd'
)
BEGIN
    CREATE TABLE [PartsTime] (
        [Id] int NOT NULL IDENTITY,
        [PlaceId] int NOT NULL,
        [Start] datetime2 NOT NULL,
        [End] datetime2 NOT NULL,
        CONSTRAINT [PK_PartsTime] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PartsTime_Places_PlaceId] FOREIGN KEY ([PlaceId]) REFERENCES [Places] ([PlaceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028201550_adddddddd'
)
BEGIN
    CREATE INDEX [IX_PartsTime_PlaceId] ON [PartsTime] ([PlaceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20241028201550_adddddddd'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241028201550_adddddddd', N'8.0.10');
END;
GO

COMMIT;
GO

