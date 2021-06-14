
/*
Content:
    We added Uuid to It-System and User as part of https://os2web.atlassian.net/browse/KITOSUDV-1731
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
    UPDATE [Itsystem]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
    
    UPDATE [User]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    -- TODO: JMO conflict here
    UPDATE [BusinessTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
END