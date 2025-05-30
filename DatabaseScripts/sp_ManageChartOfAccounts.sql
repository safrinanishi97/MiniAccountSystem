CREATE PROCEDURE sp_ManageChartOfAccounts
    @Action NVARCHAR(10),
    @AccountId INT = NULL,
    @AccountName NVARCHAR(100) = NULL,
    @ParentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'CREATE'
    BEGIN
        INSERT INTO ChartOfAccounts (Name, ParentId)
        VALUES (@AccountName, @ParentId)
    END
    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE ChartOfAccounts
        SET Name = @AccountName, ParentId = @ParentId
        WHERE Id = @AccountId
    END
    ELSE IF @Action = 'DELETE'
    BEGIN
        DELETE FROM ChartOfAccounts WHERE Id = @AccountId
    END
END