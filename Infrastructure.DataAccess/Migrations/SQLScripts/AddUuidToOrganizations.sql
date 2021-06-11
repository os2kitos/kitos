/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1712
 
Content:
    Ensures all organizations have uuid before changing the column to required, non nullable
*/

BEGIN
    UPDATE [Organization]
        SET Uuid = NEWID()
    where uuid is null;
END



