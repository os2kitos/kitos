/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-2468
 
Content:
    Sets the current default municipality
*/

BEGIN
    UPDATE [Organization]
    SET IsDefaultOrganization = 1
    WHERE [Name] = 'Fælles Kommune';
END