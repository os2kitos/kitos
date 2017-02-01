/*Sets all local sample space to active*/
DECLARE @uId int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[Organization]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
/*Organization Roles*/
INSERT INTO [dbo].[LocalOrganizationUnitRoles](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[OrganizationUnitRoles]

/*Project Roles*/
INSERT INTO [dbo].[LocalItProjectRoles](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItProjectRoles]

/*Project types*/
INSERT INTO [dbo].[LocalItProjectTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItProjectTypes]

	  /*Project goaltypes*/
INSERT INTO [dbo].[LocalGoalTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[GoalTypes]

	  /*ItSystem Roles*/
INSERT INTO [dbo].[LocalItSystemRoles](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItSystemRoles]

	  	  /*ItSystem Applikationstyper*/
INSERT INTO [dbo].[LocalItSystemTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItSystemTypes]

	  	  /*ItSystem Forretningstyper*/
INSERT INTO [dbo].[LocalBusinessTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[BusinessTypes]

	  	  	  /*ItSystem Arkiveringstyper*/
INSERT INTO [dbo].[LocalArchiveTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ArchiveTypes]

	  	  	  	  /*ItSystem Arkiveringssted*/
INSERT INTO [dbo].[LocalArchiveLocations](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ArchiveLocations]

	  	  	  /*ItSystem Datatyper*/
INSERT INTO [dbo].[LocalDataTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[DataTypes]

	  	  	  	  /*ItSystem Frekvenser*/
INSERT INTO [dbo].[LocalFrequencyTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[FrequencyTypes]

	  	  	  	  /*ItSystem Snitfladetyper*/
INSERT INTO [dbo].[LocalItInterfaceTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItInterfaceTypes]

	  	  	  	  /*ItSystem Grænseflader*/
INSERT INTO [dbo].[LocalInterfaceTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[InterfaceTypes]

	  	  	  	  /*ItSystem Metoder*/
INSERT INTO [dbo].[LocalMethodTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[MethodTypes]

	  	  	  	  /*ItSystem Personfølsomme data*/
INSERT INTO [dbo].[LocalSensitiveDataTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[SensitiveDataTypes]

	  	  	  	  /*ItSystem TSA*/
INSERT INTO [dbo].[LocalTsaTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[TsaTypes]

	  	  	  	  /*ItKontrakt Roller*/
INSERT INTO [dbo].[LocalItContractRoles](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItContractRoles]

	  	  	  	  	  /*ItKontrakt typer*/
INSERT INTO [dbo].[LocalItContractTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItContractTypes]

	  	  	  	  	  /*ItKontraktskabeloner*/
INSERT INTO [dbo].[LocalItContractTemplateTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ItContractTemplateTypes]

	  	  	  	  	  /*ItKontrakt indkøbsformer*/
INSERT INTO [dbo].[LocalPurchaseFormTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[PurchaseFormTypes]

	  	  	  	  	  /*ItKontrakt betallingsmodeller*/
INSERT INTO [dbo].[LocalPaymentModelTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[PaymentModelTypes]

	  	  	  	  	  /*ItKontrakt Aftaleelementer*/
INSERT INTO [dbo].[LocalAgreementElementTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[AgreementElementTypes]

	  	  	  	  	  /*ItKontrakt Option forlæng*/
INSERT INTO [dbo].[LocalOptionExtendTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[OptionExtendTypes]

	  	  	  	  	  /*ItKontrakt Betallngsfrekvenser*/
INSERT INTO [dbo].[LocalPaymentFreqencyTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[PaymentFreqencyTypes]

	  	  	  	  	  /*ItKontrakt Pris regulering*/
INSERT INTO [dbo].[LocalPriceRegulationTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[PriceRegulationTypes]

	  	  	  	  	  /*ItKontrakt Indkøbsstrategier*/
INSERT INTO [dbo].[LocalProcurementStrategyTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[ProcurementStrategyTypes]

	  	  	  	  	  /*ItKontrakt Opsigelsesfrister*/
INSERT INTO [dbo].[LocalTerminationDeadlineTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[TerminationDeadlineTypes]

	  	  	  	  	  /*ItKontrakt Overtagelsesprøver*/
INSERT INTO [dbo].[LocalHandoverTrialTypes](       
       [OrganizationId]
      ,[OptionId]
      ,[ObjectOwnerId]
      ,[LastChanged]
      ,[LastChangedByUserId]
      ,[IsActive])
SELECT        
       [OrganizationId] = @uId
      ,[OptionId] = [Id]
      ,[ObjectOwnerId] = 1
      ,[LastChanged] = GETDATE()
      ,[LastChangedByUserId] = 1
      ,[IsActive] = 1
	  FROM [dbo].[HandoverTrialTypes]

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName
