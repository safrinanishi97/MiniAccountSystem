CREATE PROCEDURE sp_GetAllVouchersWithDetails
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        vm.VoucherId,
        vm.VoucherType,
        vm.VoucherDate,
        vm.ReferenceNo,
        vm.CreatedBy, 
        vm.CreatedDate,
        vm.UpdatedBy,     
        vm.UpdatedDate,    
        vd.VoucherDetailId,
        vd.AccountId,
        ca.Name AS AccountName,
        vd.DebitAmount,
        vd.CreditAmount
    FROM 
        VoucherMaster vm
    JOIN 
        VoucherDetails vd ON vm.VoucherId = vd.VoucherId
    JOIN
        ChartOfAccounts ca ON vd.AccountId = ca.Id
    ORDER BY
        vm.VoucherDate DESC, vm.VoucherId DESC;
END