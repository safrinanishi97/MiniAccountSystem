CREATE TABLE [dbo].[VoucherEntries] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [VoucherId]    INT             NULL,
    [AccountId]    INT             NULL,
    [DebitAmount]  DECIMAL (18, 2) NULL,
    [CreditAmount] DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([VoucherId]) REFERENCES [dbo].[Vouchers] ([Id])
);
