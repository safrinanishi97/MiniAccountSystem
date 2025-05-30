CREATE TABLE [dbo].[ChartOfAccounts] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (100) NOT NULL,
    [ParentId] INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([ParentId]) REFERENCES [dbo].[ChartOfAccounts] ([Id])
);
