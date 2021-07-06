/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1932
 
Content:
    Ensures all org units have unique uuid after adding the column
*/

BEGIN
    UPDATE [OrganizationUnit]
        SET Uuid = NEWID()
    where Uuid is null OR Uuid = '00000000-0000-0000-0000-000000000000';
END



