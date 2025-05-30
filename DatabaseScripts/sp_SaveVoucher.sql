CREATE PROCEDURE sp_SaveVoucher
    @Action VARCHAR(10), -- 'Create' or 'Update'
    @VoucherId INT = NULL,
    @VoucherType VARCHAR(20),
    @VoucherDate DATE,
    @ReferenceNo VARCHAR(50),
    @VoucherDetails VoucherDetailType READONLY,
    @CreatedBy VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Action = 'Create'
        BEGIN
            INSERT INTO VoucherMaster (VoucherType, VoucherDate, ReferenceNo, CreatedBy, CreatedDate)
            VALUES (@VoucherType, @VoucherDate, @ReferenceNo, @CreatedBy, GETDATE());

            SET @VoucherId = SCOPE_IDENTITY();
        END
        ELSE IF @Action = 'Update'
        BEGIN
            -- Update master
            UPDATE VoucherMaster
            SET VoucherType = @VoucherType,
                VoucherDate = @VoucherDate,
                ReferenceNo = @ReferenceNo
            WHERE VoucherId = @VoucherId;

            -- Delete old details
            DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;
        END

        -- Insert new details
        INSERT INTO VoucherDetails (VoucherId, AccountId, DebitAmount, CreditAmount)
        SELECT @VoucherId, AccountId, DebitAmount, CreditAmount
        FROM @VoucherDetails;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        -- Raise error
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;