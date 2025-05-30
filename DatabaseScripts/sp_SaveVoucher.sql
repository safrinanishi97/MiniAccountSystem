CREATE PROCEDURE sp_SaveVoucher
    @VoucherType NVARCHAR(20),
    @ReferenceNo NVARCHAR(50),
    @VoucherDate DATE,
    @Entries VoucherEntryType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewVoucherId INT;
    DECLARE @CalculatedTotalDebit DECIMAL(18,2);
    DECLARE @CalculatedTotalCredit DECIMAL(18,2);

    -- Calculate total debit and total credit from entries
    SELECT
        @CalculatedTotalDebit = ISNULL(SUM(DebitAmount), 0),
        @CalculatedTotalCredit = ISNULL(SUM(CreditAmount), 0)
    FROM
        @Entries;

    -- Insert into Vouchers table
    INSERT INTO Vouchers (VoucherType, ReferenceNo, VoucherDate, TotalDebit, TotalCredit, CreatedDate)
    VALUES (@VoucherType, @ReferenceNo, @VoucherDate, @CalculatedTotalDebit, @CalculatedTotalCredit, GETDATE());

    SET @NewVoucherId = SCOPE_IDENTITY(); -- Get the ID of the newly inserted voucher

    -- Insert into VoucherEntries table
    INSERT INTO VoucherEntries (VoucherId, AccountId, DebitAmount, CreditAmount)
    SELECT
        @NewVoucherId,
        AccountId,
        DebitAmount,
        CreditAmount
    FROM
        @Entries;

END;