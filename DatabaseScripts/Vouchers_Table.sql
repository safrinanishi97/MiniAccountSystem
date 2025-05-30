CREATE TABLE [dbo].[Vouchers] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [VoucherType] NVARCHAR (50)   NOT NULL,
    [ReferenceNo] NVARCHAR (100)  NOT NULL,
    [VoucherDate] DATE            NOT NULL,
    [TotalDebit]  DECIMAL (18, 2) NOT NULL,
    [TotalCredit] DECIMAL (18, 2) NOT NULL,
    [CreatedDate] DATETIME        DEFAULT (getdate()) NULL,
    [CreatedBy]   NVARCHAR (256)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

