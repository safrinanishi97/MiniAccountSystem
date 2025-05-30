CREATE PROCEDURE sp_DeleteVoucher
    @VoucherId INT
AS
BEGIN
    DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;
    DELETE FROM VoucherMaster WHERE VoucherId = @VoucherId;
END
