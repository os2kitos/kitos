/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-3402
 
Content:
    Anonymizes users than Local admin and Api user
*/

BEGIN
    -- Make sure the temp table doesn't exist
    If(OBJECT_ID('tempdb..#idsToPreserve') Is Not Null)
    Begin
        Drop Table #idsToPreserve
    End

    CREATE TABLE #idsToPreserve(
        Id int
    )

    -- Insert values from the csv file
    BULK INSERT #idsToPreserve
        FROM 'D:\FileName.csv'
    WITH
    (
        FIRSTROW = 1,
        DATAFILETYPE='widechar', -- UTF-16
        FIELDTERMINATOR = ';',
        ROWTERMINATOR = '\n',
        TABLOCK,
        KEEPNULLS -- Treat empty fields as NULLs.
    )

    -- Delete users not in csv file
    UPDATE [User]
    SET 
        Name = 'Slettet bruger',
        LockedOutDate = GETDATE(),
        Email = CONVERT(NVARCHAR(36), NEWID()) + '_deleted_user@kitos.dk',
        PhoneNumber = null,
        LastName = '',
    Password = NEWID(),
    DeletedDate = GETDATE(),
    Deleted = 1,
    IsGlobalAdmin = 0,
    HasApiAccess = 0,
    HasStakeHolderAccess = 0
    FROM [User]
    WHERE Deleted = 0
    AND Id NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM DataProcessingRegistrationRights
	WHERE UserId NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM ItContractRights
	WHERE UserId NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM ItSystemRights
	WHERE UserId NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM OrganizationUnitRights
	WHERE UserId NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM OrganizationRights
	WHERE UserId NOT IN (SELECT * FROM #idsToPreserve);

	DELETE FROM SsoUserIdentities
	WHERE User_Id NOT IN (SELECT * FROM #idsToPreserve);


    -- Make sure the temp table doesn't exist
    If(OBJECT_ID('tempdb..#idsToPreserve') Is Not Null)
    Begin
        Drop Table #idsToPreserve
    End
END