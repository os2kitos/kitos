/*Oprydning af valgmuligheder i udfaldsrum*/
USE kitos
DELETE [dbo].[ItContractTemplateTypes]
WHERE Id = 11

ALTER TABLE [dbo].[ItSystemUsage] NOCHECK CONSTRAINT "FK_dbo.ItSystemUsage_dbo.ArchiveTypes_ArchiveTypeId"
DELETE [dbo].[ArchiveTypes]
WHERE Id IN (1,2,3,4,5)
ALTER TABLE [dbo].[ItSystemUsage] CHECK CONSTRAINT "FK_dbo.ItSystemUsage_dbo.ArchiveTypes_ArchiveTypeId"

/*Clean up*/
DELETE [dbo].[ItProjectTypes]
WHERE [Id] in (11,10,9,8,25)

DELETE [dbo].[BusinessTypes]
WHERE [Id] in (22,19,27,28)

/*Kontrakt paymenttypes*/
/*delte icens - flatrate*/
DELETE [dbo].[PaymentModelTypes]
WHERE [Id] = 11

/*Org roller*/
DELETE [dbo].[OrganizationUnitRoles]
WHERE [Id] in (8,1,6,7)

DELETE [dbo].[ItProjectRoles]
WHERE [Id] = 6

DELETE [dbo].[ItContractTemplateTypes]
WHERE [dbo].[ItContractTemplateTypes].Id = 10