
/*
Content:
    https://os2web.atlassian.net/browse/KITOSUDV-2354
    Patching old broken external reference registrtions where lastmodifiedbyuser was null
*/

BEGIN
UPDATE [ExternalReferences]
      SET [LastChangedByUserId] = [ExternalReferences].ObjectOwnerId
 WHERE LastChangedByUserId is null
END
