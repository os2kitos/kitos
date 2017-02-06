/*Udfaldsrum*/
USE kitos
/*Org Roles*/
UPDATE [dbo].[OrganizationUnitRoles]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Projekt*/
/*Roller*/
UPDATE [dbo].[ItProjectRoles]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Typer*/
UPDATE [dbo].[ItProjectTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*MålTyper*/
UPDATE [dbo].[GoalTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*System*/
/*Roller*/
UPDATE [dbo].[ItSystemRoles]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Applikationstyper*/
UPDATE [dbo].[ItSystemTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Forretningstyper*/
UPDATE [dbo].[BusinessTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Arkiveringstyper*/
UPDATE [dbo].[ArchiveTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Arkiveringssted*/
UPDATE [dbo].[ArchiveLocations]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Datatyper*/
UPDATE [dbo].[DataTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Frekvenser*/
UPDATE [dbo].[FrequencyTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Snitfladetyper*/
UPDATE [dbo].[ItInterfaceTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Grænseflader*/
UPDATE [dbo].[InterfaceTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Metoder*/
UPDATE [dbo].[MethodTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Personfølsomme data*/
UPDATE [dbo].[SensitiveDataTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*TSA*/
UPDATE [dbo].[TsaTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Kontrakter*/
/*Roller*/
UPDATE [dbo].[ItContractRoles]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Kontrakttyper*/
UPDATE [dbo].[ItContractTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Kontraktskabeloner*/
UPDATE [dbo].[ItContractTemplateTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Indkøbsformer*/
UPDATE [dbo].[PurchaseFormTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Betalingsmodeller*/
UPDATE [dbo].[PaymentModelTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Aftaleelementer*/
UPDATE [dbo].[AgreementElementTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Option forlæng*/
UPDATE [dbo].[OptionExtendTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Betalingsfrekvenser*/
UPDATE [dbo].[PaymentFreqencyTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Pris reguleringer*/
UPDATE [dbo].[PriceRegulationTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Indkøbsstrategier*/
UPDATE [dbo].[ProcurementStrategyTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Opsigelsesfrister*/
UPDATE [dbo].[TerminationDeadlineTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Overtagelsesprøver*/
UPDATE [dbo].[HandoverTrialTypes]
SET [IsEnabled] = 1, [IsLocallyAvailable] = 1

/*Oprydning af udfaldsrum*/
UPDATE [kitos_old].[dbo].[ItContract]
SET [ContractTemplateId] = 1
WHERE [ContractTemplateId] = 11


/******* SET [Priority] FIX *********/

/*Org Unit Roles*/
UPDATE [dbo].[OrganizationUnitRoles]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[OrganizationUnitRoles]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[OrganizationUnitRoles])

UPDATE [dbo].[OrganizationUnitRoles]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Projekt*/
/*Roller*/
UPDATE [dbo].[ItProjectRoles]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItProjectRoles]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItProjectRoles])

UPDATE [dbo].[ItProjectRoles]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Typer*/
UPDATE [dbo].[ItProjectTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItProjectTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItProjectTypes])

UPDATE [dbo].[ItProjectTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*MålTyper*/
UPDATE [dbo].[GoalTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[GoalTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[GoalTypes])

UPDATE [dbo].[GoalTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*System*/
/*Roller*/
UPDATE [dbo].[ItSystemRoles]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItSystemRoles]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItSystemRoles])

UPDATE [dbo].[ItSystemRoles]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Applikationstyper*/
UPDATE [dbo].[ItSystemTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItSystemTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItSystemTypes])

UPDATE [dbo].[ItSystemTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Forretningstyper*/
UPDATE [dbo].[BusinessTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[BusinessTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[BusinessTypes])

UPDATE [dbo].[BusinessTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Arkiveringstyper*/
UPDATE [dbo].[ArchiveTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ArchiveTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ArchiveTypes])

UPDATE [dbo].[ArchiveTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Arkiveringssted*/
UPDATE [dbo].[ArchiveLocations]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ArchiveLocations]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ArchiveLocations])

UPDATE [dbo].[ArchiveLocations]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Datatyper*/
UPDATE [dbo].[DataTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[DataTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[DataTypes])

UPDATE [dbo].[DataTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Frekvenser*/
UPDATE [dbo].[FrequencyTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[FrequencyTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[FrequencyTypes])

UPDATE [dbo].[FrequencyTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Snitfladetyper*/
UPDATE [dbo].[ItInterfaceTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItInterfaceTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItInterfaceTypes])

UPDATE [dbo].[ItInterfaceTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Grænseflader*/
UPDATE [dbo].[InterfaceTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[InterfaceTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[InterfaceTypes])

UPDATE [dbo].[InterfaceTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO

/*Metoder*/
UPDATE [dbo].[MethodTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[MethodTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[MethodTypes])

UPDATE [dbo].[MethodTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Personfølsomme data*/
UPDATE [dbo].[SensitiveDataTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[SensitiveDataTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[SensitiveDataTypes])

UPDATE [dbo].[SensitiveDataTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*TSA*/
UPDATE [dbo].[TsaTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[TsaTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[TsaTypes])

UPDATE [dbo].[TsaTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Kontrakter*/
/*Roller*/
UPDATE [dbo].[ItContractRoles]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItContractRoles]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItContractRoles])

UPDATE [dbo].[ItContractRoles]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Kontrakttyper*/
UPDATE [dbo].[ItContractTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItContractTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItContractTypes])

UPDATE [dbo].[ItContractTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Kontraktskabeloner*/
UPDATE [dbo].[ItContractTemplateTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ItContractTemplateTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ItContractTemplateTypes])

UPDATE [dbo].[ItContractTemplateTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Indkøbsformer*/
UPDATE [dbo].[PurchaseFormTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[PurchaseFormTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[PurchaseFormTypes])

UPDATE [dbo].[PurchaseFormTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Betalingsmodeller*/
UPDATE [dbo].[PaymentModelTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[PaymentModelTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[PaymentModelTypes])

UPDATE [dbo].[PaymentModelTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Aftaleelementer*/
UPDATE [dbo].[AgreementElementTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[AgreementElementTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[AgreementElementTypes])

UPDATE [dbo].[AgreementElementTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Option forlæng*/
UPDATE [dbo].[OptionExtendTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[OptionExtendTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[OptionExtendTypes])

UPDATE [dbo].[OptionExtendTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Betalingsfrekvenser*/
UPDATE [dbo].[PaymentFreqencyTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[PaymentFreqencyTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[PaymentFreqencyTypes])

UPDATE [dbo].[PaymentFreqencyTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Pris reguleringer*/
UPDATE [dbo].[PriceRegulationTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[PriceRegulationTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[PriceRegulationTypes])

UPDATE [dbo].[PriceRegulationTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Indkøbsstrategier*/
UPDATE [dbo].[ProcurementStrategyTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[ProcurementStrategyTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[ProcurementStrategyTypes])

UPDATE [dbo].[ProcurementStrategyTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Opsigelsesfrister*/
UPDATE [dbo].[TerminationDeadlineTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[TerminationDeadlineTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[TerminationDeadlineTypes])

UPDATE [dbo].[TerminationDeadlineTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
/*Overtagelsesprøver*/
UPDATE [dbo].[HandoverTrialTypes]
SET [Priority] = 0
GO
 
DECLARE @uId int, @i int

DECLARE cursorName CURSOR

LOCAL SCROLL STATIC

FOR

SELECT Id FROM [dbo].[HandoverTrialTypes]

OPEN cursorName

FETCH NEXT FROM cursorName

INTO @uId

WHILE @@FETCH_STATUS = 0

BEGIN
SET @i = (SELECT MAX([Priority])
FROM [dbo].[HandoverTrialTypes])

UPDATE [dbo].[HandoverTrialTypes]
SET [Priority] = @i + 1
WHERE Id = @uId

FETCH NEXT FROM cursorName
INTO @uId

END

CLOSE cursorName

DEALLOCATE cursorName

GO
