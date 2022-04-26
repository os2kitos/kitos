/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-2468
 
Content:
    Ensures that all task refs point to the root org unit of the default organization
*/

BEGIN
    UPDATE [TaskRef]
    SET OwnedByOrganizationUnitId = (select Id from OrganizationUnit where ParentId IS NULL AND OrganizationId in (select Id from Organization where IsDefaultOrganization = 1))
END