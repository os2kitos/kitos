/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1739
 
Content:
    Ensures all users have unique uuid after adding the column
*/

BEGIN
    UPDATE [User]
        SET Uuid = NEWID()
    where Uuid is null OR Uuid = '00000000-0000-0000-0000-000000000000';

    UPDATE [ItInterface]
        SET Uuid = NEWID()
    where Uuid is null OR Uuid = '00000000-0000-0000-0000-000000000000';
END



