/*Projekt roller dublikerede*/
/*IT konsulent*/
UPDATE [dbo].[ItProjectRights]
SET [RoleId] = 23
WHERE [RoleId] = 6

/*Projekt typer*/
/*Fællesoffentlig*/
UPDATE [dbo].[ItProject]
SET [ItProjectTypeId] = 1
WHERE [ItProjectTypeId] = 11

/*Fælleskommunal*/
UPDATE [dbo].[ItProject]
SET [ItProjectTypeId] = 2
WHERE [ItProjectTypeId] = 10

/*Tværkommunal*/
UPDATE [dbo].[ItProject]
SET [ItProjectTypeId] = 4
WHERE [ItProjectTypeId] = 9

/*Lokal*/
UPDATE [dbo].[ItProject]
SET [ItProjectTypeId] = 3
WHERE [ItProjectTypeId] = 8

/*-----------------------------------------------------------------*/
/*System typer dublikerede*/
/*Design...*/
UPDATE [dbo].[ItSystem]
SET [BusinessTypeId] = 1
WHERE [BusinessTypeId] in (19,22)

/*Selvbetjening og indberetning*/
UPDATE [dbo].[ItSystem]
SET [BusinessTypeId] = 4
WHERE [BusinessTypeId] IN (20,21)

UPDATE [dbo].[ItSystem]
SET [BusinessTypeId] = 12
WHERE [BusinessTypeId] = 27

/*-----------------------------------------------------------------*/
/*Orgnaisations roller dublikerede*/
/*Ressourceperson*/
UPDATE [dbo].[OrganizationUnitRights]
SET [RoleId] = 2
WHERE [RoleId] = 8

/*Chef*/
UPDATE [dbo].[OrganizationUnitRights]
SET [RoleId] = 23
WHERE [RoleId] = 1

/*Leder*/
UPDATE [dbo].[OrganizationUnitRights]
SET [RoleId] = 22
WHERE [RoleId] = 6

/*Direktør*/
UPDATE [dbo].[OrganizationUnitRights]
SET [RoleId] = 24
WHERE [RoleId] = 7

UPDATE [dbo].[Organization]
SET [AccessModifier] = '1'

UPDATE [dbo].[Advice] 
SET [StopDate] = [AlarmDate]

GO

/*-----------------------------------------------------------------*/
/*Migrate helptext*/
/*SET IDENTITY_INSERT [kitos_old].[dbo].[HelpTexts] ON skal klades seperat*/

SET IDENTITY_INSERT [dbo].[HelpTexts] ON

INSERT INTO [dbo].[HelpTexts]([Id]
      ,[Title]
      ,[Description]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Key])
SELECT [Id]
      ,[Title]
      ,[Description]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Key] FROM [kitos_old].[dbo].[HelpTexts]
	  
SET IDENTITY_INSERT [dbo].[HelpTexts] OFF

GO

/*Migrate rapport*/
ALTER TABLE [dbo].[Reports] NOCHECK CONSTRAINT "FK_dbo.Reports_dbo.User_LastChangedByUserId"
ALTER TABLE [dbo].[Reports] NOCHECK CONSTRAINT "FK_dbo.Reports_dbo.User_ObjectOwnerId"

SET IDENTITY_INSERT [dbo].[Reports] ON

INSERT INTO [dbo].[Reports]([Id]
      ,[Name]
      ,[Description]
      ,[CategoryTypeId]
      ,[OrganizationId]
      ,[Definition]
      ,[AccessModifier]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT [Id]
      ,[Name]
      ,[Description]
      ,[CategoryTypeId]
      ,[OrganizationId]
      ,[Definition]
      ,[AccessModifier]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId] FROM [kitos_old].[dbo].[Reports]

SET IDENTITY_INSERT [dbo].[Reports] OFF

ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT "FK_dbo.Reports_dbo.User_LastChangedByUserId"
ALTER TABLE [dbo].[Reports] NOCHECK CONSTRAINT "FK_dbo.Reports_dbo.User_ObjectOwnerId"

GO

/*Migrate rapport types check database, same data        no need        */
--SET IDENTITY_INSERT [dbo].[ReportCategoryTypes] ON
--INSERT INTO [dbo].[ReportCategoryTypes]([Id]
--      ,[Name]
--      ,[IsSuggestion]
--      ,[ObjectOwnerId]
--      ,[LastChanged]
--      ,[LastChangedByUserId]
--      ,[Description]
--      ,[IsObligatory]
--      ,[IsEnabled]
--      ,[IsLocallyAvailable]
--      ,[Priority])
--SELECT [Id]
--      ,[Name]
--      ,[IsSuggestion]
--      ,[ObjectOwnerId]
--      ,[LastChanged]
--      ,[LastChangedByUserId]
--      ,[Description]
--      ,[IsObligatory]
--      ,[IsEnabled]
--      ,[IsLocallyAvailable]
--      ,[Priority] FROM [kitos_old].[dbo].[ReportCategoryTypes]
	  
--SET IDENTITY_INSERT [dbo].[ReportCategoryTypes] OFF

SET IDENTITY_INSERT [dbo].[BusinessTypes] ON

ALTER TABLE [dbo].[ItSystem] NOCHECK CONSTRAINT "FK_dbo.ItSystem_dbo.BusinessTypes_BusinessTypeId"

DELETE FROM [dbo].[BusinessTypes]

INSERT INTO [dbo].[BusinessTypes]([Id]
      ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority])
SELECT [Id]
      ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority] FROM [kitos_old].[dbo].[BusinessTypes]

	  INSERT INTO [dbo].[BusinessTypes]([Id]
      ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority])
SELECT 29 
      ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority] FROM [kitos_old].[dbo].[BusinessTypes]
	  WHERE Id = 28

	  UPDATE [dbo].[ItSystem]
  SET [BusinessTypeId] = null
  WHERE [BusinessTypeId] = 28

ALTER TABLE [dbo].[ItSystem] CHECK CONSTRAINT "FK_dbo.ItSystem_dbo.BusinessTypes_BusinessTypeId"

SET IDENTITY_INSERT [dbo].[BusinessTypes] OFF

GO

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i system roller fra preprod i opdateret db*/
UPDATE [dbo].[ItSystemRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItSystemRoles] OLD
WHERE [dbo].[ItSystemRoles].Id = OLD.Id

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i arkiveringssted fra preprod i opdateret db*/
UPDATE [dbo].[ItSystemRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItSystemRoles] OLD
WHERE [dbo].[ItSystemRoles].Id = OLD.Id

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i Personfølsomt data fra preprod i opdateret db*/
UPDATE [dbo].[SensitiveDataTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[SensitiveDataTypes] OLD
WHERE [dbo].[SensitiveDataTypes].Id = 4 AND OLD.Id = 3

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i kontrakt roller fra preprod i opdateret db*/
UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 1 AND OLD.Id = 1

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 2 AND OLD.Id = 2

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 3 AND OLD.Id = 3

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 4 AND OLD.Id = 4

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 5 AND OLD.Id = 5

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 6 AND OLD.Id = 6

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 7 AND OLD.Id = 7

UPDATE [dbo].[ItContractRoles]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractRoles] OLD
WHERE [dbo].[ItContractRoles].Id = 8 AND OLD.Id = 11

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i kontrakttyper fra preprod i opdateret db*/
UPDATE [dbo].[ItContractTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractTypes] OLD
WHERE [dbo].[ItContractTypes].Id = OLD.Id

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i kontraktskabeloner fra preprod i opdateret db*/
UPDATE [dbo].[ItContractTemplateTypes]
SET Name = OLD.[Name]
FROM [kitos_old].[dbo].[ItContractTemplateTypes] OLD
WHERE [dbo].[ItContractTemplateTypes].Id = 11

UPDATE [dbo].[ItContractTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ItContractTypes] OLD
WHERE [dbo].[ItContractTypes].Id = OLD.Id

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i indkøbsformer fra preprod i opdateret db*/
UPDATE [dbo].[PurchaseFormTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[PurchaseFormTypes] OLD
WHERE [dbo].[PurchaseFormTypes].Id = OLD.Id

GO

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i betalingstyper fra preprod i opdateret db*/

ALTER TABLE [dbo].[ItContract] NOCHECK CONSTRAINT "FK_dbo.ItContract_dbo.PaymentModelTypes_PaymentModelId"

DELETE [dbo].[PaymentModelTypes]

ALTER TABLE [dbo].[ItContract] CHECK CONSTRAINT "FK_dbo.ItContract_dbo.PaymentModelTypes_PaymentModelId"

SET IDENTITY_INSERT [dbo].[PaymentModelTypes] ON

INSERT INTO [dbo].[PaymentModelTypes]([Id]
	  ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority])
SELECT [Id]
	  ,[Name]
      ,[IsSuggestion]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[Description]
      ,[IsObligatory]
      ,[IsEnabled]
      ,[IsLocallyAvailable]
      ,[Priority] FROM [kitos_old].[dbo].[PaymentModelTypes]

SET IDENTITY_INSERT [dbo].[PaymentModelTypes] OFF

GO

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i indkøbsstrategi fra preprod i opdateret db*/
UPDATE [dbo].[ProcurementStrategyTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[ProcurementStrategyTypes] OLD
WHERE [dbo].[ProcurementStrategyTypes].Id = OLD.Id

/*-----------------------------------------------------------------*/
/*Indsætter beskrivivelse i Overtagelsesprøver fra preprod i opdateret db*/
UPDATE [dbo].[HandoverTrialTypes]
SET Description = OLD.[Description]
FROM [kitos_old].[dbo].[HandoverTrialTypes] OLD
WHERE [dbo].[HandoverTrialTypes].Id = OLD.Id

GO

/*-----------------------------------------------------------------*/
ALTER TABLE [kitos_old].[dbo].[ItSystemUsage] NOCHECK CONSTRAINT "FK_dbo.ItSystemUsage_dbo.ArchiveLocations_ArchiveLocationId"

DELETE [kitos_old].[dbo].[ArchiveLocations]
WHERE [Id] = 1

DELETE [kitos_old].[dbo].[ArchiveLocations]
WHERE [Id] = 2

ALTER TABLE [kitos_old].[dbo].[ItSystemUsage] CHECK CONSTRAINT "FK_dbo.ItSystemUsage_dbo.ArchiveLocations_ArchiveLocationId"

DELETE [dbo].[ArchiveLocations]

SET IDENTITY_INSERT [dbo].[ArchiveLocations] ON

INSERT INTO [dbo].[ArchiveLocations]([Id]
      ,[Name]
      ,[IsLocallyAvailable]
      ,[IsObligatory]
      ,[IsSuggestion]
      ,[Description]
      ,[IsEnabled]
      ,[Priority]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT [Id]
      ,[Name]
      ,[IsLocallyAvailable]
      ,[IsObligatory]
      ,[IsSuggestion]
      ,[Description]
      ,[IsEnabled]
      ,[Priority]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId] FROM [kitos_old].[dbo].[ArchiveLocations]

SET IDENTITY_INSERT [dbo].[ArchiveLocations] OFF

GO

/*Opretter advis sendt*/
INSERT INTO [dbo].[AdviceSents]([AdviceSentDate]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT [SentDate]
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,GETDATE()
	  ,1
FROM [dbo].[Advice]
WHERE [SentDate] IS NOT NULL

GO

---------------------------------------------------
/*[AdviceUserRelations]*/

DELETE [dbo].[AdviceUserRelations]

GO

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Kontraktejer'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 1 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Kontraktejer'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 1 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Kontraktmanager'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 2 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Kontraktmanager'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 2 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Juridisk rådgiver'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 3 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Juridisk rådgiver'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 3 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Konsulent'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 4 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Konsulent'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 4 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Fakturamodtager'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 5 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Fakturamodtager'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 5 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Budgetansvarlig'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 6 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Budgetansvarlig'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 6 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Medunderskriver'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 7 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Medunderskriver'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 7 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Arkivansvarlig'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 8 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Arkivansvarlig'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 8 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Projektleder på udbud'
	  ,3
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [ReceiverId] = 9 AND [ReceiverId] IS NOT NULL

INSERT INTO [dbo].[AdviceUserRelations]([Name]
      ,[RecieverType]
      ,[RecpientType]
      ,[AdviceId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId])
SELECT 'Projektleder på udbud'
	  ,2
	  ,0
	  ,[Id]
	  ,[ObjectOwnerId]
	  ,[LastChanged]
      ,[LastChangedByUserId]
FROM [dbo].[Advice]
WHERE [CarbonCopyReceiverId] = 9 AND [ReceiverId] IS NOT NULL

GO