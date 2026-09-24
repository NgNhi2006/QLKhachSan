SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=2) THROW 51000,N'Cần nâng cấp phiên bản 2 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=3)
BEGIN
    DECLARE @roleConstraint sysname;
    SELECT @roleConstraint=name FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'dbo.Users') AND definition LIKE '%Reception%' AND definition LIKE '%Admin%';
    IF @roleConstraint IS NOT NULL
    BEGIN
        DECLARE @dropRole nvarchar(300)=N'ALTER TABLE dbo.Users DROP CONSTRAINT '+QUOTENAME(@roleConstraint);
        EXEC sys.sp_executesql @dropRole;
    END;
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_RoleV3 CHECK(Role IN('Admin','Reception','Accountant','Manager'));
    ALTER TABLE dbo.Users ADD Archived bit NOT NULL CONSTRAINT DF_Users_Archived DEFAULT 0;
    DECLARE @usernameConstraint sysname;
    SELECT @usernameConstraint=kc.name FROM sys.key_constraints kc
    JOIN sys.index_columns ic ON ic.object_id=kc.parent_object_id AND ic.index_id=kc.unique_index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE kc.parent_object_id=OBJECT_ID(N'dbo.Users') AND kc.type='UQ' AND c.name='Username';
    IF @usernameConstraint IS NOT NULL
    BEGIN
        DECLARE @dropUsername nvarchar(300)=N'ALTER TABLE dbo.Users DROP CONSTRAINT '+QUOTENAME(@usernameConstraint);
        EXEC sys.sp_executesql @dropUsername;
    END;
    EXEC(N'CREATE UNIQUE INDEX UX_Users_ActiveUsername ON dbo.Users(Username) WHERE Archived=0');
    -- Retain referenced user rows for invoices and audit, but revoke every old login.
    EXEC(N'UPDATE dbo.Users SET Archived=1,Active=0,SecurityVersion=SecurityVersion+1,LockedUntil=NULL WHERE Archived=0');
    INSERT dbo.SchemaVersion VALUES(3);
END;
COMMIT;
