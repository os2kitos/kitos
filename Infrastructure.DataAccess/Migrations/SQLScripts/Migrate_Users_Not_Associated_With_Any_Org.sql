/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-3422
 
Content:
    Sets Users with no assossiated Organizations as deleted
*/

UPDATE [User]
SET 
	Name = 'Slettet bruger',
	LockedOutDate = GETDATE(),
	Email = NEWID() + '_deleted_user@kitos.dk',
	PhoneNumber = null,
	LastName = (CASE
				WHEN LastName IS NULL THEN ''
				ELSE LastName
			   END) + ' (SLETTET)',
	DeletedDate = GETDATE(),
	Deleted = 1,
	IsGlobalAdmin = 0,
	HasApiAccess = 0,
	HasStakeHolderAccess = 0
FROM [User] T0 
LEFT JOIN  OrganizationRights T1 
ON T0.Id = T1.UserId
WHERE T1.Id IS NULL	