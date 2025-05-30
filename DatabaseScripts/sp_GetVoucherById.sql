CREATE PROCEDURE sp_GetVoucherById
    @VoucherId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- First result: Voucher master
    SELECT 
        VoucherId,
        VoucherType,
        VoucherDate,
        ReferenceNo
    FROM VoucherMaster
    WHERE VoucherId = @VoucherId;

    -- Second result: Voucher details
    SELECT 
        vd.VoucherDetailId,
        vd.AccountId,
        coa.Name AS AccountName,
        vd.DebitAmount,
        vd.CreditAmount
    FROM VoucherDetails vd
    INNER JOIN ChartOfAccounts coa ON vd.AccountId = coa.Id
    WHERE vd.VoucherId = @VoucherId;
END;