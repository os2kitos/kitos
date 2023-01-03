
/*
Content:
    The model for sub data processors have changed due to https://os2web.atlassian.net/browse/KITOSUDV-3562
    Moving from a many-many table between dpr and organization into a 1..* relationship between dpr and the new sub data processor table
*/

BEGIN
    INSERT INTO [dbo].[SubDataProcessors] (OrganizationId, DataProcessingRegistrationId) 
    SELECT Organization_Id, DataProcessingRegistration_Id
    FROM [dbo].[DataProcessingRegistrationOrganization1];
END