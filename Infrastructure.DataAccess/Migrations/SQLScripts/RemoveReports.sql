
/*
Content:
    Reports were removed in https://os2web.atlassian.net/browse/KITOSUDV-1290
    This script cleans the database of: 
    - "Preferred start preference set to reports.overview
    - Role assignments of "Report Admin"
*/

BEGIN
    UPDATE [User]
        SET DefaultUserStartPreference = NULL
    WHERE DefaultUserStartPreference = 'reports.overview';

    DELETE
        FROM [OrganizationRights] 
    WHERE [Role] = 6;
END