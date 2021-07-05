
/*
Content:
    We added Uuid to It-Project as part of https://os2web.atlassian.net/browse/KITOSUDV-1945
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
    UPDATE [ItProject]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
END