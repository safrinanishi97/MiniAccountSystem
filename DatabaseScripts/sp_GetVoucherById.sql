CREATE PROCEDURE sp_GetVoucherById
    @VoucherId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- First result: Voucher master with Created and Updated info
    SELECT 
        VoucherId,
        VoucherType,
        VoucherDate,
        ReferenceNo,
        CreatedBy,
        CreatedDate,
        UpdatedBy,       
        UpdatedDate     
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