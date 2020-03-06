/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-601
 
Content:
    Sets archive duty field to NULL on all rows in the ItSystem table
*/

BEGIN
    UPDATE [ItSystem] 
    SET ArchiveDuty = NULL;
END