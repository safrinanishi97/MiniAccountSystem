CREATE TABLE [dbo].[VoucherDetails] (
    [VoucherDetailId] INT             IDENTITY (1, 1) NOT NULL,
    [VoucherId]       INT             NOT NULL,
    [AccountId]       INT             NOT NULL,
    [DebitAmount]     DECIMAL (18, 2) NOT NULL,
    [CreditAmount]    DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([VoucherDetailId] ASC),
    FOREIGN KEY ([VoucherId]) REFERENCES [dbo].[VoucherMaster] ([VoucherId])
);

