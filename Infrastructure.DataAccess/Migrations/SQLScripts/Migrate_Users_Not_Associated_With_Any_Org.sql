/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-3422
 
Content:
    Sets Users with no assossiated Organizations as deleted
*/

BEGIN
	If(OBJECT_ID('tempdb..#get_userIds_to_delete') IS NOT NULL)
	Begin
		Drop Table #get_userIds_to_delete
	End

	CREATE TABLE #get_userIds_to_delete 
	(
		Id int
	)

	INSERT INTO #get_userIds_to_delete
	SELECT T0.Id
	FROM [User] T0 
	LEFT JOIN  OrganizationRights T1 
	ON T0.Id = T1.UserId
	WHERE 
		T1.Id IS NULL AND T0.Deleted = 0 AND
		T0.IsGlobalAdmin = 0;

	UPDATE [User]
	SET 
		Name = 'Slettet bruger',
		LockedOutDate = GETDATE(),
		Email = CONVERT(NVARCHAR(36), NEWID()) + '_deleted_user@kitos.dk',
		PhoneNumber = null,
		LastName = '',
		DeletedDate = GETDATE(),
		Deleted = 1,
		IsGlobalAdmin = 0,
		HasApiAccess = 0,
		HasStakeHolderAccess = 0
	WHERE Id IN (SELECT Id FROM #get_userIds_to_delete);

	DELETE FROM DataProcessingRegistrationRights
	WHERE UserId IN (SELECT Id FROM #get_userIds_to_delete);

	DELETE FROM ItContractRights
	WHERE UserId IN (SELECT Id FROM #get_userIds_to_delete);

	DELETE FROM ItSystemRights
	WHERE UserId IN (SELECT Id FROM #get_userIds_to_delete);

	DELETE FROM OrganizationUnitRights
	WHERE UserId IN (SELECT Id FROM #get_userIds_to_delete);

	DELETE FROM SsoUserIdentities
	WHERE User_Id IN (SELECT Id FROM #get_userIds_to_delete);

	If(OBJECT_ID('tempdb..#get_userIds_to_delete') IS NOT NULL)
	Begin
		Drop Table #get_userIds_to_delete
	End
END