/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-602
 
Content:
    Migrates old data from the "Url" field to a new external reference by the following logic:

    If there are already references associated with the system, no data is migrated.
    If there is valid data in the field and no existing references, the date is copied to a new reference associated with the system 
    and afterwards the new reference is set as the "master" reference on the system.
*/

BEGIN
  
END