CREATE TABLE [dbo].[ModuleAccess] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [RoleName]   NVARCHAR (50)  NULL,
    [ModuleName] NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

