﻿/*
Content:
    We added Uuid to External Reference as part of https://os2web.atlassian.net/browse/KITOSUDV-3812
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
    UPDATE[ExternalReferences]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
END
