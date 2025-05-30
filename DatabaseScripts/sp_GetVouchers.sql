CREATE PROCEDURE sp_GetVouchers
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        VoucherType,
        ReferenceNo,
        VoucherDate,
        TotalDebit,
        TotalCredit,
        CreatedDate -- Assuming you add a CreatedDate column to Vouchers table
    FROM
        Vouchers
    ORDER BY
        VoucherDate DESC, Id DESC; -- Newest vouchers first
END;