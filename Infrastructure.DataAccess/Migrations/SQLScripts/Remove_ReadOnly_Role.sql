/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-660
 
Content:
    Removes all organization rights where (role > 6)
    This removes the removed role (ReadOnly) and GlobalAdmin (which is not a valid org role but a boolean on the user)
*/

BEGIN
    DELETE
    FROM [OrganizationRights] 
    WHERE [Role] > 6;
END