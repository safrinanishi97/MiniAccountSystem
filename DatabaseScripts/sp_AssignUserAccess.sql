CREATE PROCEDURE sp_AssignUserAccess
    @RoleName NVARCHAR(50),
    @ModuleName NVARCHAR(100)
AS
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM ModuleAccess WHERE RoleName = @RoleName AND ModuleName = @ModuleName
    )
    BEGIN
        INSERT INTO ModuleAccess (RoleName, ModuleName)
        VALUES (@RoleName, @ModuleName)
    END
END