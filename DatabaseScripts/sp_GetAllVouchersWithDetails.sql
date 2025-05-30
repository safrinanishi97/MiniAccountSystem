CREATE PROCEDURE sp_GetAllVouchersWithDetails
AS
BEGIN
    SELECT 
        vm.VoucherId,
        vm.VoucherDate,
        vm.ReferenceNo,
        vm.VoucherType,
        vd.VoucherDetailId,
        vd.AccountId,
        coa.Name AS AccountName,
        vd.DebitAmount,
        vd.CreditAmount
    FROM VoucherMaster vm
    INNER JOIN VoucherDetails vd ON vm.VoucherId = vd.VoucherId
    INNER JOIN ChartOfAccounts coa ON vd.AccountId = coa.Id
    ORDER BY vm.VoucherDate DESC, vm.VoucherId DESC
END
