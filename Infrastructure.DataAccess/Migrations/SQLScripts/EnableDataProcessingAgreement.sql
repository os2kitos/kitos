/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1082
 
Content:
    Sets DataProcessingAgreement view to be visible on every org in the start.
*/

BEGIN
UPDATE [Config]
SET ShowDataProcessingAgreement = 1
END



