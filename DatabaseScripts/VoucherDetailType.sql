CREATE TYPE [dbo].[VoucherDetailType] AS TABLE (
    [AccountId]    INT             NULL,
    [DebitAmount]  DECIMAL (18, 2) NULL,
    [CreditAmount] DECIMAL (18, 2) NULL);

