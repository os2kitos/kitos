/*

Content:
 Migrates existing system and contract GDPR data to Data Processing Registrations according to the logic described in https://os2web.atlassian.net/browse/KITOSUDV-1271

*/
/*
	First migration situation
	Contract with ContractType as "Databehandleraftale" (5) and only DPR related data on Contract
*/

BEGIN

	DECLARE @ItContractIdsToBeDeleted_1 TABLE 
    (
		ItContractId int
    )

	INSERT INTO @ItContractIdsToBeDeleted_1
	SELECT Id
	FROM ItContract
	WHERE
		ContractTypeId = 5
		AND 
		SupplierContractSigner IS NULL
		AND 
		HasSupplierSigned = 0
		AND
		SupplierSignedDate IS NULL
		AND
		IsSigned = 0
		AND
		SignedDate IS NULL
		AND
		ResponsibleOrganizationUnitId IS NULL
		AND
		SupplierId IS NULL
		AND
		ProcurementStrategyId IS NULL
		AND
		ProcurementPlanHalf IS NULL
		AND
		ProcurementPlanYear IS NULL
		AND
		ContractTemplateId IS NULL
		AND
		PurchaseFormId IS NULL
		AND
		Concluded IS NULL
		AND
		IrrevocableTo IS NULL
		AND
		ExpirationDate IS NULL
		AND
		Terminated IS NULL
		AND
		TerminationDeadlineId IS NULL
		AND
		OptionExtendId IS NULL
		AND
		ExtendMultiplier = 0
		AND
		OperationRemunerationBegun IS NULL
		AND 
		PaymentFreqencyId IS NULL
		AND 
		PaymentModelId IS NULL
		AND 
		PriceRegulationId IS NULL
		AND 
		Running IS NULL
		AND 
		ByEnding IS NULL
		AND 
		DurationYears IS NULL
		AND 
		DurationMonths IS NULL
		AND 
		DurationOngoing = 0
		AND 
		ContractSigner IS NULL
		AND 
		Id NOT IN (
			SELECT HandoverTrial.ItContractId
			FROM HandoverTrial
			)
		AND
		Id NOT IN (
			SELECT PaymentMilestones.ItContractId
			FROM PaymentMilestones
			)
		AND
		Id NOT IN (
			SELECT ItContractAgreementElementTypes.ItContract_Id
			FROM ItContractAgreementElementTypes
				INNER JOIN AgreementElementTypes ON Id = AgreementElementType_Id
			WHERE AgreementElementTypes.Name != 'Databehandleraftale'
			)
		AND
		Id NOT IN (
			SELECT EconomyStream.ExternPaymentForId
			FROM EconomyStream
			WHERE ExternPaymentForId IS NOT NULL
			)
		AND
		Id NOT IN (
			SELECT EconomyStream.InternPaymentForId
			FROM EconomyStream
			WHERE InternPaymentForId IS NOT NULL
			)
		AND
		Id NOT IN (
			SELECT DISTINCT SystemRelations.AssociatedContractId
			FROM SystemRelations
			WHERE AssociatedContractId IS NOT NULL
		)
		AND
		ID NOT IN (
			SELECT DISTINCT ParentId
			FROM ItContract
			WHERE ParentId IS NOT NULL
		)

	DECLARE @ContractsWithAtLeast2SystemUsages_1 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractsWithAtLeast2SystemUsages_1
	SELECT ItContractItSystemUsages.ItContractId
	FROM @ItContractIdsToBeDeleted_1 AS contractIdsToBeDeleted
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = contractIdsToBeDeleted.ItContractId
	GROUP BY ItContractItSystemUsages.ItContractId
	HAVING COUNT(*) > 1

	DECLARE @ContractsAndUsagesWith0Or1SystemUsages_1 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWith0Or1SystemUsages_1
	SELECT contractIdsToBeDeleted.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@ItContractIdsToBeDeleted_1 AS contractIdsToBeDeleted
		LEFT JOIN
		ItContractItSystemUsages ON contractIdsToBeDeleted.ItContractId = ItContractItSystemUsages.ItContractId
	WHERE contractIdsToBeDeleted.ItContractId NOT IN 
		(SELECT ContractsWithAtLeast2SystemUsages.ItContractId
		FROM @ContractsWithAtLeast2SystemUsages_1 AS ContractsWithAtLeast2SystemUsages
		)

	DECLARE @ContractsAndUsagesWithAtLeast2SystemUsages_1 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWithAtLeast2SystemUsages_1
	SELECT ItContractItSystemUsages.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM @ContractsWithAtLeast2SystemUsages_1 AS ContractsWithAtLeast2SystemUsages
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractsWithAtLeast2SystemUsages.ItContractId

	DECLARE @AllContractsWithMultipleSystemUsagesGrouped_1 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithMultipleSystemUsagesGrouped_1
	SELECT 
		ItContractId,
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM 
		@ContractsAndUsagesWithAtLeast2SystemUsages_1
		INNER JOIN 
		ItSystemUsage ON Id = ItSystemUsageId
	GROUP BY ItContractId, 
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName


	DECLARE @ContractIdWhereSystemUsagesDataIsEqual_1 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_1
	SELECT
		ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_1
	GROUP BY 
		ItContractId
	HAVING
		COUNT(*) < 2


	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_1
	SELECT ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_1
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_1
		)
	GROUP BY ItContractId
	HAVING 
		COUNT(DISTINCT case when dataProcessor IS NOT NULL AND dataProcessor != '' then dataProcessor else null end) < 2
		AND
		COUNT(  DISTINCT
		CASE WHEN dataProcessorControl = 0 OR dataProcessorControl = 1 OR dataProcessorControl = 2 THEN dataProcessorControl  
			 WHEN dataProcessorControl = 3 
				AND (
					 (dataProcessor IS NOT NULL AND dataProcessor != '')
					 OR
					 lastControl IS NOT NULL
					 OR
					 (noteUsage IS NOT NULL AND noteUsage != '')
					 OR
					 (datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '')
					 OR
					 (datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '')
				)
			THEN dataProcessorControl 
			ELSE null END) < 2
		AND
		COUNT(DISTINCT lastControl) < 2
		AND
		COUNT(DISTINCT case when noteUsage IS NOT NULL AND noteUsage != '' then noteUsage else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '' then datahandlerSupervisionDocumentationUrl else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '' then datahandlerSupervisionDocumentationUrlName else null end) < 2


	DECLARE @AllContractsWithEqualSystemGDPRData_1 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_1
	SELECT *
	FROM @AllContractsWithMultipleSystemUsagesGrouped_1
	WHERE 
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_1
		)
		AND
		(
			ItContractId NOT IN (
					SELECT ItContractId
					FROM @AllContractsWithMultipleSystemUsagesGrouped_1
					GROUP BY ItContractId
					HAVING COUNT(*) > 1
			)
			OR
			(
				(
					dataProcessor IS NOT NULL
					AND
					dataProcessor != ''
				)
				OR
				lastControl IS NOT NULL
				OR
				(
					noteUsage IS NOT NULL
					AND
					noteUsage != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrl IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrl != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrlName IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrlName != ''
				)
			)
		)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_1
	SELECT ItContractId, MAX(dataProcessor), MIN(dataProcessorControl), MAX(lastControl), MAX(noteUsage), MAX(datahandlerSupervisionDocumentationUrl), MAX(datahandlerSupervisionDocumentationUrlName)
	FROM @AllContractsWithMultipleSystemUsagesGrouped_1
	WHERE
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_1
		)
		AND
		ItContractId NOT IN (
			SELECT ItContractId
			FROM @AllContractsWithEqualSystemGDPRData_1
		)
	GROUP BY ItContractId

	DECLARE @ContractIdsWithUnequalSystemGDPRData_1 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdsWithUnequalSystemGDPRData_1
	SELECT
		ItContractId
	FROM 
		@ContractsWithAtLeast2SystemUsages_1
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_1
		)


	/*
		Migration 1 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 0 or 1 It System Usages associated.
	*/

	DECLARE @ContractMigrationData_1_1 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		ItSystemUsageId int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_1_1
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		ItSystemUsage.Id,
		ItSystemUsage.dataProcessor,
		CASE
			WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
			WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
			WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM
		@ContractsAndUsagesWith0Or1SystemUsages_1 AS ContractsWith0Or1SystemUsages
		INNER JOIN
		ItContract ON ItContract.Id = ContractsWith0Or1SystemUsages.ItContractId
		INNER JOIN
		ItSystemUsage ON ItSystemUsage.Id = ContractsWith0Or1SystemUsages.ItSystemUsageId


	DECLARE @DprIds_1_1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_1_1
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_1_1
		
	DECLARE @DprsWithForeignKeys_1_1 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_1_1 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_1_1  as context
		INNER JOIN @DprIds_1_1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_1_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_1_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_1_1

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_1_1 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_1_1 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_1_1 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_1_1
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_1_1
	WHERE
		DatahandlerSupervisionLink IS NOT NULL

			
	/*
		Migration 2 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where all system GDPR data is the same.
	*/

	DECLARE @ContractMigrationData_1_2 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_1_2
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		AllContractsWithEqualSystemGDPRData.dataProcessor,
		CASE
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 0 THEN 1
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 1 THEN 0
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		AllContractsWithEqualSystemGDPRData.lastControl,
		AllContractsWithEqualSystemGDPRData.noteUsage,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrl,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrlName
	FROM
		@AllContractsWithEqualSystemGDPRData_1 AS AllContractsWithEqualSystemGDPRData
		INNER JOIN
		ItContract ON ItContract.Id = AllContractsWithEqualSystemGDPRData.ItContractId


	DECLARE @DprIds_1_2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_1_2
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_1_2
		
	DECLARE @DprsWithForeignKeys_1_2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	

	INSERT INTO 
		@DprsWithForeignKeys_1_2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_1_2  as context
		INNER JOIN @DprIds_1_2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	
	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_1_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT 
		DprId, Id
	FROM 
		@DprsWithForeignKeys_1_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprsWithForeignKeys.DprId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_1_2 AS DprsWithForeignKeys
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = DprsWithForeignKeys.ItContractId

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_1_2 AS dprsWithForeign
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = dprsWithForeign.ItContractId
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON ItContractItSystemUsages.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)
	

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_1_2 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_1_2 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_1_2
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_1_2
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	

	

	/*
		Migration 3 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where system GDPR data is different.
	*/

	DECLARE @MigrationContext_1_3 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		SystemUsageUserId int
    )

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext_1_3 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, ContractName, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			ItContract.Name + '_' + CAST(ItSystemUsage.Id as varchar(max)),
			ItContract.ContainsDataHandlerAgreement,
			ItContract.DataHandlerAgreementUrl,
			ItContract.DataHandlerAgreementUrlName,
			ItContract.Note,
			ItSystemUsage.dataProcessor,
			CASE
				WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
				WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
				WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
				ELSE 2
			END AS OversightConcluded,
			ItSystemUsage.lastControl,
			ItSystemUsage.noteUsage,
			ItSystemUsage.datahandlerSupervisionDocumentationUrl,
			ItSystemUsage.datahandlerSupervisionDocumentationUrlName,
			ItContract.ObjectOwnerId,
			ItContract.LastChangedByUserId,
			ItSystemUsage.ObjectOwnerId
        FROM 
			@ContractIdsWithUnequalSystemGDPRData_1 AS ContractIdsWithUnequalSystemGDPRData
			INNER JOIN
			ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractIdsWithUnequalSystemGDPRData.ItContractId
			INNER JOIN 
			ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId 
			INNER JOIN 
			ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId

	

	DECLARE @DprIds_1_3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_1_3
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext_1_3 


	DECLARE @DprsWithForeignKeys_1_3 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	

	INSERT INTO 
		@DprsWithForeignKeys_1_3 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext_1_3  as context
		INNER JOIN @DprIds_1_3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_1_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_1_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_1_3

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_1_3 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_1_3 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_1_3 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_1_3
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		systemOwnerId
	FROM 
		@DprsWithForeignKeys_1_3
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


END

/*
	Third migration situation
	Contract with ContractType as NOT "Databehandleraftale" (5) and only DPR related data on Contract
*/

BEGIN

	DECLARE @ItContractIdsToBeDeleted_3 TABLE 
    (
		ItContractId int
    )

	INSERT INTO @ItContractIdsToBeDeleted_3
	SELECT Id
	FROM ItContract
	WHERE
		(
			ContractTypeId != 5
			OR
			ContractTypeId IS NULL
		)
		AND
		(
			ContainsDataHandlerAgreement != 0
			OR
			DataHandlerAgreementUrl IS NOT NULL
			OR
			DataHandlerAgreementUrlName IS NOT NULL 
		)
		AND 
		SupplierContractSigner IS NULL
		AND 
		HasSupplierSigned = 0
		AND
		SupplierSignedDate IS NULL
		AND
		IsSigned = 0
		AND
		SignedDate IS NULL
		AND
		ResponsibleOrganizationUnitId IS NULL
		AND
		SupplierId IS NULL
		AND
		ProcurementStrategyId IS NULL
		AND
		ProcurementPlanHalf IS NULL
		AND
		ProcurementPlanYear IS NULL
		AND
		ContractTemplateId IS NULL
		AND
		PurchaseFormId IS NULL
		AND
		Concluded IS NULL
		AND
		IrrevocableTo IS NULL
		AND
		ExpirationDate IS NULL
		AND
		Terminated IS NULL
		AND
		TerminationDeadlineId IS NULL
		AND
		OptionExtendId IS NULL
		AND
		ExtendMultiplier = 0
		AND
		OperationRemunerationBegun IS NULL
		AND 
		PaymentFreqencyId IS NULL
		AND 
		PaymentModelId IS NULL
		AND 
		PriceRegulationId IS NULL
		AND 
		Running IS NULL
		AND 
		ByEnding IS NULL
		AND 
		DurationYears IS NULL
		AND 
		DurationMonths IS NULL
		AND 
		DurationOngoing = 0
		AND 
		ContractSigner IS NULL
		AND 
		Id NOT IN (
			SELECT HandoverTrial.ItContractId
			FROM HandoverTrial
			)
		AND
		Id NOT IN (
			SELECT PaymentMilestones.ItContractId
			FROM PaymentMilestones
			)
		AND
		Id NOT IN (
			SELECT ItContractAgreementElementTypes.ItContract_Id
			FROM ItContractAgreementElementTypes
				INNER JOIN AgreementElementTypes ON Id = AgreementElementType_Id
			WHERE AgreementElementTypes.Name != 'Databehandleraftale'
			)
		AND
		Id NOT IN (
			SELECT EconomyStream.ExternPaymentForId
			FROM EconomyStream
			WHERE ExternPaymentForId IS NOT NULL
			)
		AND
		Id NOT IN (
			SELECT EconomyStream.InternPaymentForId
			FROM EconomyStream
			WHERE InternPaymentForId IS NOT NULL
			)
		AND
		Id NOT IN (
			SELECT DISTINCT SystemRelations.AssociatedContractId
			FROM SystemRelations
			WHERE AssociatedContractId IS NOT NULL
		)
		AND
		ID NOT IN (
			SELECT DISTINCT ParentId
			FROM ItContract
			WHERE ParentId IS NOT NULL
		)

	DECLARE @ContractsWithAtLeast2SystemUsages_3 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractsWithAtLeast2SystemUsages_3
	SELECT ItContractItSystemUsages.ItContractId
	FROM @ItContractIdsToBeDeleted_3 AS contractIdsToBeDeleted
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = contractIdsToBeDeleted.ItContractId
	GROUP BY ItContractItSystemUsages.ItContractId
	HAVING COUNT(*) > 1

	DECLARE @ContractsAndUsagesWith0Or1SystemUsages_3 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWith0Or1SystemUsages_3
	SELECT contractIdsToBeDeleted.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@ItContractIdsToBeDeleted_3 AS contractIdsToBeDeleted
		LEFT JOIN
		ItContractItSystemUsages ON contractIdsToBeDeleted.ItContractId = ItContractItSystemUsages.ItContractId
	WHERE contractIdsToBeDeleted.ItContractId NOT IN 
		(SELECT ContractsWithAtLeast2SystemUsages.ItContractId
		FROM @ContractsWithAtLeast2SystemUsages_3 AS ContractsWithAtLeast2SystemUsages
		)

	DECLARE @ContractsAndUsagesWithAtLeast2SystemUsages_3 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWithAtLeast2SystemUsages_3
	SELECT ItContractItSystemUsages.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM @ContractsWithAtLeast2SystemUsages_3 AS ContractsWithAtLeast2SystemUsages
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractsWithAtLeast2SystemUsages.ItContractId

	DECLARE @AllContractsWithMultipleSystemUsagesGrouped_3 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithMultipleSystemUsagesGrouped_3
	SELECT 
		ItContractId,
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM 
		@ContractsAndUsagesWithAtLeast2SystemUsages_3
		INNER JOIN 
		ItSystemUsage ON Id = ItSystemUsageId
	GROUP BY ItContractId, 
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName


	DECLARE @ContractIdWhereSystemUsagesDataIsEqual_3 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_3
	SELECT
		ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_3
	GROUP BY 
		ItContractId
	HAVING
		COUNT(*) < 2


	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_3
	SELECT ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_3
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_3
		)
	GROUP BY ItContractId
	HAVING 
		COUNT(DISTINCT case when dataProcessor IS NOT NULL AND dataProcessor != '' then dataProcessor else null end) < 2
		AND
		COUNT( DISTINCT
		CASE WHEN dataProcessorControl = 0 OR dataProcessorControl = 1 OR dataProcessorControl = 2 THEN dataProcessorControl  
			 WHEN dataProcessorControl = 3 
				AND (
					 (dataProcessor IS NOT NULL AND dataProcessor != '')
					 OR
					 lastControl IS NOT NULL
					 OR
					 (noteUsage IS NOT NULL AND noteUsage != '')
					 OR
					 (datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '')
					 OR
					 (datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '')
				)
			THEN dataProcessorControl 
			ELSE null END) < 2
		AND
		COUNT(DISTINCT lastControl) < 2
		AND
		COUNT(DISTINCT case when noteUsage IS NOT NULL AND noteUsage != '' then noteUsage else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '' then datahandlerSupervisionDocumentationUrl else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '' then datahandlerSupervisionDocumentationUrlName else null end) < 2


	DECLARE @AllContractsWithEqualSystemGDPRData_3 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_3
	SELECT *
	FROM @AllContractsWithMultipleSystemUsagesGrouped_3
	WHERE 
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_3
		)
		AND
		(
			ItContractId NOT IN (
					SELECT ItContractId
					FROM @AllContractsWithMultipleSystemUsagesGrouped_3
					GROUP BY ItContractId
					HAVING COUNT(*) > 1
			)
			OR
			(
				(
					dataProcessor IS NOT NULL
					AND
					dataProcessor != ''
				)
				OR
				lastControl IS NOT NULL
				OR
				(
					noteUsage IS NOT NULL
					AND
					noteUsage != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrl IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrl != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrlName IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrlName != ''
				)
			)
		)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_3
	SELECT ItContractId, MAX(dataProcessor), MIN(dataProcessorControl), MAX(lastControl), MAX(noteUsage), MAX(datahandlerSupervisionDocumentationUrl), MAX(datahandlerSupervisionDocumentationUrlName)
	FROM @AllContractsWithMultipleSystemUsagesGrouped_3
	WHERE
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_3
		)
		AND
		ItContractId NOT IN (
			SELECT ItContractId
			FROM @AllContractsWithEqualSystemGDPRData_3
		)
	GROUP BY ItContractId

	DECLARE @ContractIdsWithUnequalSystemGDPRData_3 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdsWithUnequalSystemGDPRData_3
	SELECT
		ItContractId
	FROM 
		@ContractsWithAtLeast2SystemUsages_3
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_3
		)

	/*
		Migration 1 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 0 or 1 It System Usages associated.
	*/

	DECLARE @ContractMigrationData_3_1 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		ItSystemUsageId int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_3_1
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		ItSystemUsage.Id,
		ItSystemUsage.dataProcessor,
		CASE
			WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
			WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
			WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM
		@ContractsAndUsagesWith0Or1SystemUsages_3 AS ContractsWith0Or1SystemUsages
		INNER JOIN
		ItContract ON ItContract.Id = ContractsWith0Or1SystemUsages.ItContractId
		INNER JOIN
		ItSystemUsage ON ItSystemUsage.Id = ContractsWith0Or1SystemUsages.ItSystemUsageId


	DECLARE @DprIds_3_1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_3_1
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_3_1
		
	DECLARE @DprsWithForeignKeys_3_1 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_3_1 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_3_1  as context
		INNER JOIN @DprIds_3_1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_3_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_3_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_3_1

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_3_1 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_3_1 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_3_1 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_3_1
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_3_1
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Migration 2 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where all system GDPR data is the same.
	*/

	DECLARE @ContractMigrationData_3_2 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_3_2
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		AllContractsWithEqualSystemGDPRData.dataProcessor,
		CASE
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 0 THEN 1
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 1 THEN 0
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		AllContractsWithEqualSystemGDPRData.lastControl,
		AllContractsWithEqualSystemGDPRData.noteUsage,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrl,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrlName
	FROM
		@AllContractsWithEqualSystemGDPRData_3 AS AllContractsWithEqualSystemGDPRData
		INNER JOIN
		ItContract ON ItContract.Id = AllContractsWithEqualSystemGDPRData.ItContractId


	DECLARE @DprIds_3_2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_3_2
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_3_2
		
	DECLARE @DprsWithForeignKeys_3_2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_3_2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_3_2  as context
		INNER JOIN @DprIds_3_2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_3_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_3_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprsWithForeignKeys.DprId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_3_2 AS DprsWithForeignKeys
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = DprsWithForeignKeys.ItContractId

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_3_2 AS dprsWithForeign
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = dprsWithForeign.ItContractId
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON ItContractItSystemUsages.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_3_2 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_3_2 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_3_2
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_3_2
	WHERE
		DatahandlerSupervisionLink IS NOT NULL



	/*
		Migration 3 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where system GDPR data is different.
	*/

	DECLARE @MigrationContext_3_3 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		SystemUsageUserId int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext_3_3 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, ContractName, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			ItContract.Name + '_' + CAST(ItSystemUsage.Id as varchar(max)),
			ItContract.ContainsDataHandlerAgreement,
			ItContract.DataHandlerAgreementUrl,
			ItContract.DataHandlerAgreementUrlName,
			ItContract.Note,
			ItSystemUsage.dataProcessor,
			CASE
				WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
				WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
				WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
				ELSE 2
			END AS OversightConcluded,
			ItSystemUsage.lastControl,
			ItSystemUsage.noteUsage,
			ItSystemUsage.datahandlerSupervisionDocumentationUrl,
			ItSystemUsage.datahandlerSupervisionDocumentationUrlName,
			ItContract.ObjectOwnerId,
			ItContract.LastChangedByUserId,
			ItSystemUsage.ObjectOwnerId
        FROM 
			@ContractIdsWithUnequalSystemGDPRData_3 AS ContractIdsWithUnequalSystemGDPRData
			INNER JOIN
			ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractIdsWithUnequalSystemGDPRData.ItContractId
			INNER JOIN 
			ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId 
			INNER JOIN 
			ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId



	DECLARE @DprIds_3_3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_3_3
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext_3_3 


	DECLARE @DprsWithForeignKeys_3_3 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_3_3 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext_3_3  as context
		INNER JOIN @DprIds_3_3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_3_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_3_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_3_3

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_3_3 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_3_3 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_3_3 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_3_3
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		systemOwnerId
	FROM 
		@DprsWithForeignKeys_3_3
	WHERE
		DatahandlerSupervisionLink IS NOT NULL

END

/*
	Handle deletion of affected contracts
*/

BEGIN

	/*
		Prepare contracts to be deleted by removing child's foreign keys and then
		delete contracts as they were created as DPR's in migration situation 1
	*/

	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_1
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_1
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_1
	)



	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_2
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_2
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_2
	)



	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_3
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_3
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_1_3
	)


	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_1
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_1
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_1
	)



	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_2
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_2
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_2
	)



	UPDATE ItContract
	SET ParentId = null
	WHERE
		ParentId IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_3
	)

	DELETE
	FROM
		ItContractAgreementElementTypes
	WHERE
		ItContract_Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_3
	)

	DELETE
	FROM 
		ItContract
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_3_3
	)


END


/*
	Second migration situation
	Contract with ContractType as "Databehandleraftale" (5) and DPR related 0 + other data on Contract
*/

BEGIN

	DECLARE @ItContractIdsToBeKept_2 TABLE 
    (
		ItContractId int
    )

	INSERT INTO @ItContractIdsToBeKept_2
	SELECT Id
	FROM ItContract
	WHERE
		ContractTypeId = 5
		AND 
		(
			SupplierContractSigner IS NOT NULL
			OR 
			HasSupplierSigned != 0
			OR
			SupplierSignedDate IS NOT NULL
			OR
			IsSigned != 0
			OR
			SignedDate IS NOT NULL
			OR
			ResponsibleOrganizationUnitId IS NOT NULL
			OR
			SupplierId IS NOT NULL
			OR
			ProcurementStrategyId IS NOT NULL
			OR
			ProcurementPlanHalf IS NOT NULL
			OR
			ProcurementPlanYear IS NOT NULL
			OR
			ContractTemplateId IS NOT NULL
			OR
			PurchaseFormId IS NOT NULL
			OR
			Concluded IS NOT NULL
			OR
			IrrevocableTo IS NOT NULL
			OR
			ExpirationDate IS NOT NULL
			OR
			Terminated IS NOT NULL
			OR
			TerminationDeadlineId IS NOT NULL
			OR
			OptionExtendId IS NOT NULL
			OR
			ExtendMultiplier != 0
			OR
			OperationRemunerationBegun IS NOT NULL
			OR 
			PaymentFreqencyId IS NOT NULL
			OR 
			PaymentModelId IS NOT NULL
			OR 
			PriceRegulationId IS NOT NULL
			OR 
			Running IS NOT NULL
			OR 
			ByEnding IS NOT NULL
			OR 
			DurationYears IS NOT NULL
			OR 
			DurationMonths IS NOT NULL
			OR 
			DurationOngoing != 0
			OR 
			ContractSigner IS NOT NULL
			OR 
			Id IN (
				SELECT HandoverTrial.ItContractId
				FROM HandoverTrial
				)
			OR
			Id IN (
				SELECT PaymentMilestones.ItContractId
				FROM PaymentMilestones
				)
			OR
			Id IN (
			SELECT ItContractAgreementElementTypes.ItContract_Id
			FROM ItContractAgreementElementTypes
				INNER JOIN AgreementElementTypes ON Id = AgreementElementType_Id
			WHERE AgreementElementTypes.Name != 'Databehandleraftale'
			)
			OR
			Id IN (
				SELECT EconomyStream.ExternPaymentForId
				FROM EconomyStream
				WHERE ExternPaymentForId IS NOT NULL
				)
			OR
			Id IN (
				SELECT EconomyStream.InternPaymentForId
				FROM EconomyStream
				WHERE InternPaymentForId IS NOT NULL
				)
			OR
			Id IN (
				SELECT DISTINCT SystemRelations.AssociatedContractId
				FROM SystemRelations
				WHERE AssociatedContractId IS NOT NULL
			)
			OR
			ID IN (
				SELECT DISTINCT ParentId
				FROM ItContract
				WHERE ParentId IS NOT NULL
			)
		)

	DECLARE @ContractsWithAtLeast2SystemUsages_2 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractsWithAtLeast2SystemUsages_2
	SELECT ItContractItSystemUsages.ItContractId
	FROM @ItContractIdsToBeKept_2 AS contractIdsToBeKept
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = contractIdsToBeKept.ItContractId
	GROUP BY ItContractItSystemUsages.ItContractId
	HAVING COUNT(*) > 1

	DECLARE @ContractsAndUsagesWith0Or1SystemUsages_2 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWith0Or1SystemUsages_2
	SELECT contractIdsToBeKept.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@ItContractIdsToBeKept_2 AS contractIdsToBeKept
		LEFT JOIN
		ItContractItSystemUsages ON contractIdsToBeKept.ItContractId = ItContractItSystemUsages.ItContractId
	WHERE contractIdsToBeKept.ItContractId NOT IN 
		(SELECT ContractsWithAtLeast2SystemUsages.ItContractId
		FROM @ContractsWithAtLeast2SystemUsages_2 AS ContractsWithAtLeast2SystemUsages
		)

	DECLARE @ContractsAndUsagesWithAtLeast2SystemUsages_2 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWithAtLeast2SystemUsages_2
	SELECT ItContractItSystemUsages.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM @ContractsWithAtLeast2SystemUsages_2 AS ContractsWithAtLeast2SystemUsages
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractsWithAtLeast2SystemUsages.ItContractId

	DECLARE @AllContractsWithMultipleSystemUsagesGrouped_2 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithMultipleSystemUsagesGrouped_2
	SELECT 
		ItContractId,
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM 
		@ContractsAndUsagesWithAtLeast2SystemUsages_2
		INNER JOIN 
		ItSystemUsage ON Id = ItSystemUsageId
	GROUP BY ItContractId, 
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName


	DECLARE @ContractIdWhereSystemUsagesDataIsEqual_2 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_2
	SELECT
		ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_2
	GROUP BY 
		ItContractId
	HAVING
		COUNT(*) < 2


	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_2
	SELECT ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_2
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_2
		)
	GROUP BY ItContractId
	HAVING 
		COUNT(DISTINCT case when dataProcessor IS NOT NULL AND dataProcessor != '' then dataProcessor else null end) < 2
		AND
		COUNT( DISTINCT
		CASE WHEN dataProcessorControl = 0 OR dataProcessorControl = 1 OR dataProcessorControl = 2 THEN dataProcessorControl  
			 WHEN dataProcessorControl = 3 
				AND (
					 (dataProcessor IS NOT NULL AND dataProcessor != '')
					 OR
					 lastControl IS NOT NULL
					 OR
					 (noteUsage IS NOT NULL AND noteUsage != '')
					 OR
					 (datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '')
					 OR
					 (datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '')
				)
			THEN dataProcessorControl 
			ELSE null END) < 2
		AND
		COUNT(DISTINCT lastControl) < 2
		AND
		COUNT(DISTINCT case when noteUsage IS NOT NULL AND noteUsage != '' then noteUsage else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '' then datahandlerSupervisionDocumentationUrl else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '' then datahandlerSupervisionDocumentationUrlName else null end) < 2


	DECLARE @AllContractsWithEqualSystemGDPRData_2 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_2
	SELECT *
	FROM @AllContractsWithMultipleSystemUsagesGrouped_2
	WHERE 
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_2
		)
		AND
		(
			ItContractId NOT IN (
					SELECT ItContractId
					FROM @AllContractsWithMultipleSystemUsagesGrouped_2
					GROUP BY ItContractId
					HAVING COUNT(*) > 1
			)
			OR
			(
				(
					dataProcessor IS NOT NULL
					AND
					dataProcessor != ''
				)
				OR
				lastControl IS NOT NULL
				OR
				(
					noteUsage IS NOT NULL
					AND
					noteUsage != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrl IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrl != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrlName IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrlName != ''
				)
			)
		)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_2
	SELECT ItContractId, MAX(dataProcessor), MIN(dataProcessorControl), MAX(lastControl), MAX(noteUsage), MAX(datahandlerSupervisionDocumentationUrl), MAX(datahandlerSupervisionDocumentationUrlName)
	FROM @AllContractsWithMultipleSystemUsagesGrouped_2
	WHERE
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_2
		)
		AND
		ItContractId NOT IN (
			SELECT ItContractId
			FROM @AllContractsWithEqualSystemGDPRData_2
		)
	GROUP BY ItContractId


	DECLARE @ContractIdsWithUnequalSystemGDPRData_2 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdsWithUnequalSystemGDPRData_2
	SELECT
		ItContractId
	FROM 
		@ContractsWithAtLeast2SystemUsages_2
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_2
		)

	/*
		Migration 1 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 0 or 1 It System Usages associated.
	*/

	DECLARE @ContractMigrationData_2_1 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		ItSystemUsageId int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_2_1
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		ItSystemUsage.Id,
		ItSystemUsage.dataProcessor,
		CASE
			WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
			WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
			WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM
		@ContractsAndUsagesWith0Or1SystemUsages_2 AS ContractsWith0Or1SystemUsages
		INNER JOIN
		ItContract ON ItContract.Id = ContractsWith0Or1SystemUsages.ItContractId
		INNER JOIN
		ItSystemUsage ON ItSystemUsage.Id = ContractsWith0Or1SystemUsages.ItSystemUsageId


	DECLARE @DprIds_2_1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_2_1
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_2_1
		
	DECLARE @DprsWithForeignKeys_2_1 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_2_1 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_2_1  as context
		INNER JOIN @DprIds_2_1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_2_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_2_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_2_1

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_2_1 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_2_1 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_2_1 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_2_1
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_2_1
	WHERE
		DatahandlerSupervisionLink IS NOT NULL




	/*
		Migration 2 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where all system GDPR data is the same.
	*/

	DECLARE @ContractMigrationData_2_2 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_2_2
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		AllContractsWithEqualSystemGDPRData.dataProcessor,
		CASE
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 0 THEN 1
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 1 THEN 0
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		AllContractsWithEqualSystemGDPRData.lastControl,
		AllContractsWithEqualSystemGDPRData.noteUsage,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrl,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrlName
	FROM
		@AllContractsWithEqualSystemGDPRData_2 AS AllContractsWithEqualSystemGDPRData
		INNER JOIN
		ItContract ON ItContract.Id = AllContractsWithEqualSystemGDPRData.ItContractId


	DECLARE @DprIds_2_2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_2_2
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_2_2
		
	DECLARE @DprsWithForeignKeys_2_2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_2_2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_2_2  as context
		INNER JOIN @DprIds_2_2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_2_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_2_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprsWithForeignKeys.DprId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_2_2 AS DprsWithForeignKeys
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = DprsWithForeignKeys.ItContractId

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_2_2 AS dprsWithForeign
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = dprsWithForeign.ItContractId
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON ItContractItSystemUsages.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_2_2 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_2_2 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_2_2
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_2_2
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Migration 3 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where system GDPR data is different.
	*/

	DECLARE @MigrationContext_2_3 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		SystemUsageUserId int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext_2_3 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, ContractName, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			ItContract.Name + '_' + CAST(ItSystemUsage.Id as varchar(max)),
			ItContract.ContainsDataHandlerAgreement,
			ItContract.DataHandlerAgreementUrl,
			ItContract.DataHandlerAgreementUrlName,
			ItContract.Note,
			ItSystemUsage.dataProcessor,
			CASE
				WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
				WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
				WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
				ELSE 2
			END AS OversightConcluded,
			ItSystemUsage.lastControl,
			ItSystemUsage.noteUsage,
			ItSystemUsage.datahandlerSupervisionDocumentationUrl,
			ItSystemUsage.datahandlerSupervisionDocumentationUrlName,
			ItContract.ObjectOwnerId,
			ItContract.LastChangedByUserId,
			ItSystemUsage.ObjectOwnerId
        FROM 
			@ContractIdsWithUnequalSystemGDPRData_2 AS ContractIdsWithUnequalSystemGDPRData
			INNER JOIN
			ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractIdsWithUnequalSystemGDPRData.ItContractId
			INNER JOIN 
			ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId 
			INNER JOIN 
			ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId


	DECLARE @DprIds_2_3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_2_3
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext_2_3 


	DECLARE @DprsWithForeignKeys_2_3 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_2_3 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext_2_3  as context
		INNER JOIN @DprIds_2_3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_2_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_2_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_2_3

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_2_3 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_2_3 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_2_3 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_2_3
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		systemOwnerId
	FROM 
		@DprsWithForeignKeys_2_3
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


END




/*
	Fourth migration situation
	Contract with ContractType as NOT "Databehandleraftale" (5) and DPR related 0 + other data on Contract
*/

BEGIN

	DECLARE @ItContractIdsToBeKept_4 TABLE 
    (
		ItContractId int
    )

	INSERT INTO @ItContractIdsToBeKept_4
	SELECT Id
	FROM ItContract
	WHERE
		(
			ContractTypeId != 5
			OR
			ContractTypeId IS NULL
		)
		AND 
		(
			ContainsDataHandlerAgreement != 0
			OR
			DataHandlerAgreementUrl IS NOT NULL
			OR
			DataHandlerAgreementUrlName IS NOT NULL 
		)
		AND
		(
			SupplierContractSigner IS NOT NULL
			OR 
			HasSupplierSigned != 0
			OR
			SupplierSignedDate IS NOT NULL
			OR
			IsSigned != 0
			OR
			SignedDate IS NOT NULL
			OR
			ResponsibleOrganizationUnitId IS NOT NULL
			OR
			SupplierId IS NOT NULL
			OR
			ProcurementStrategyId IS NOT NULL
			OR
			ProcurementPlanHalf IS NOT NULL
			OR
			ProcurementPlanYear IS NOT NULL
			OR
			ContractTemplateId IS NOT NULL
			OR
			PurchaseFormId IS NOT NULL
			OR
			Concluded IS NOT NULL
			OR
			IrrevocableTo IS NOT NULL
			OR
			ExpirationDate IS NOT NULL
			OR
			Terminated IS NOT NULL
			OR
			TerminationDeadlineId IS NOT NULL
			OR
			OptionExtendId IS NOT NULL
			OR
			ExtendMultiplier != 0
			OR
			OperationRemunerationBegun IS NOT NULL
			OR 
			PaymentFreqencyId IS NOT NULL
			OR 
			PaymentModelId IS NOT NULL
			OR 
			PriceRegulationId IS NOT NULL
			OR 
			Running IS NOT NULL
			OR 
			ByEnding IS NOT NULL
			OR 
			DurationYears IS NOT NULL
			OR 
			DurationMonths IS NOT NULL
			OR 
			DurationOngoing != 0
			OR 
			ContractSigner IS NOT NULL
			OR 
			Id IN (
				SELECT HandoverTrial.ItContractId
				FROM HandoverTrial
				)
			OR
			Id IN (
				SELECT PaymentMilestones.ItContractId
				FROM PaymentMilestones
				)
			OR
			Id IN (
			SELECT ItContractAgreementElementTypes.ItContract_Id
			FROM ItContractAgreementElementTypes
				INNER JOIN AgreementElementTypes ON Id = AgreementElementType_Id
			WHERE AgreementElementTypes.Name != 'Databehandleraftale'
			)
			OR
			Id IN (
				SELECT EconomyStream.ExternPaymentForId
				FROM EconomyStream
				WHERE ExternPaymentForId IS NOT NULL
				)
			OR
			Id IN (
				SELECT EconomyStream.InternPaymentForId
				FROM EconomyStream
				WHERE InternPaymentForId IS NOT NULL
				)
			OR
			Id IN (
				SELECT DISTINCT SystemRelations.AssociatedContractId
				FROM SystemRelations
				WHERE AssociatedContractId IS NOT NULL
			)
			OR
			ID IN (
				SELECT DISTINCT ParentId
				FROM ItContract
				WHERE ParentId IS NOT NULL
			)
		)

	DECLARE @ContractsWithAtLeast2SystemUsages_4 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractsWithAtLeast2SystemUsages_4
	SELECT ItContractItSystemUsages.ItContractId
	FROM @ItContractIdsToBeKept_4 AS contractIdsToBeKept
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = contractIdsToBeKept.ItContractId
	GROUP BY ItContractItSystemUsages.ItContractId
	HAVING COUNT(*) > 1

	DECLARE @ContractsAndUsagesWith0Or1SystemUsages_4 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWith0Or1SystemUsages_4
	SELECT contractIdsToBeKept.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@ItContractIdsToBeKept_4 AS contractIdsToBeKept
		LEFT JOIN
		ItContractItSystemUsages ON contractIdsToBeKept.ItContractId = ItContractItSystemUsages.ItContractId
	WHERE contractIdsToBeKept.ItContractId NOT IN 
		(SELECT ContractsWithAtLeast2SystemUsages.ItContractId
		FROM @ContractsWithAtLeast2SystemUsages_4 AS ContractsWithAtLeast2SystemUsages
		)

	DECLARE @ContractsAndUsagesWithAtLeast2SystemUsages_4 TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWithAtLeast2SystemUsages_4
	SELECT ItContractItSystemUsages.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM @ContractsWithAtLeast2SystemUsages_4 AS ContractsWithAtLeast2SystemUsages
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractsWithAtLeast2SystemUsages.ItContractId

	DECLARE @AllContractsWithMultipleSystemUsagesGrouped_4 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithMultipleSystemUsagesGrouped_4
	SELECT 
		ItContractId,
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM 
		@ContractsAndUsagesWithAtLeast2SystemUsages_4
		INNER JOIN 
		ItSystemUsage ON Id = ItSystemUsageId
	GROUP BY ItContractId, 
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName


	DECLARE @ContractIdWhereSystemUsagesDataIsEqual_4 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_4
	SELECT
		ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_4
	GROUP BY 
		ItContractId
	HAVING
		COUNT(*) < 2


	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual_4
	SELECT ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped_4
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_4
		)
	GROUP BY ItContractId
	HAVING 
		COUNT(DISTINCT case when dataProcessor IS NOT NULL AND dataProcessor != '' then dataProcessor else null end) < 2
		AND
		COUNT( DISTINCT
		CASE WHEN dataProcessorControl = 0 OR dataProcessorControl = 1 OR dataProcessorControl = 2 THEN dataProcessorControl  
			 WHEN dataProcessorControl = 3 
				AND (
					 (dataProcessor IS NOT NULL AND dataProcessor != '')
					 OR
					 lastControl IS NOT NULL
					 OR
					 (noteUsage IS NOT NULL AND noteUsage != '')
					 OR
					 (datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '')
					 OR
					 (datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '')
				)
			THEN dataProcessorControl 
			ELSE null END) < 2
		AND
		COUNT(DISTINCT lastControl) < 2
		AND
		COUNT(DISTINCT case when noteUsage IS NOT NULL AND noteUsage != '' then noteUsage else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrl IS NOT NULL AND datahandlerSupervisionDocumentationUrl != '' then datahandlerSupervisionDocumentationUrl else null end) < 2
		AND
		COUNT(DISTINCT case when datahandlerSupervisionDocumentationUrlName IS NOT NULL AND datahandlerSupervisionDocumentationUrlName != '' then datahandlerSupervisionDocumentationUrlName else null end) < 2


	DECLARE @AllContractsWithEqualSystemGDPRData_4 TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_4
	SELECT *
	FROM @AllContractsWithMultipleSystemUsagesGrouped_4
	WHERE 
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_4
		)
		AND
		(
			ItContractId NOT IN (
					SELECT ItContractId
					FROM @AllContractsWithMultipleSystemUsagesGrouped_4
					GROUP BY ItContractId
					HAVING COUNT(*) > 1
			)
			OR
			(
				(
					dataProcessor IS NOT NULL
					AND
					dataProcessor != ''
				)
				OR
				lastControl IS NOT NULL
				OR
				(
					noteUsage IS NOT NULL
					AND
					noteUsage != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrl IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrl != ''
				)
				OR
				(
					datahandlerSupervisionDocumentationUrlName IS NOT NULL
					AND
					datahandlerSupervisionDocumentationUrlName != ''
				)
			)
		)

	INSERT INTO @AllContractsWithEqualSystemGDPRData_4
	SELECT ItContractId, MAX(dataProcessor), MIN(dataProcessorControl), MAX(lastControl), MAX(noteUsage), MAX(datahandlerSupervisionDocumentationUrl), MAX(datahandlerSupervisionDocumentationUrlName)
	FROM @AllContractsWithMultipleSystemUsagesGrouped_4
	WHERE
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual_4
		)
		AND
		ItContractId NOT IN (
			SELECT ItContractId
			FROM @AllContractsWithEqualSystemGDPRData_4
		)
	GROUP BY ItContractId

	DECLARE @ContractIdsWithUnequalSystemGDPRData_4 TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdsWithUnequalSystemGDPRData_4
	SELECT
		ItContractId
	FROM 
		@ContractsWithAtLeast2SystemUsages_4
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual_4
		)

	/*
		Migration 1 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 0 or 1 It System Usages associated.
	*/

	DECLARE @ContractMigrationData_4_1 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		ItSystemUsageId int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_4_1
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		ItSystemUsage.Id,
		ItSystemUsage.dataProcessor,
		CASE
			WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
			WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
			WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM
		@ContractsAndUsagesWith0Or1SystemUsages_4 AS ContractsWith0Or1SystemUsages
		INNER JOIN
		ItContract ON ItContract.Id = ContractsWith0Or1SystemUsages.ItContractId
		INNER JOIN
		ItSystemUsage ON ItSystemUsage.Id = ContractsWith0Or1SystemUsages.ItSystemUsageId


	DECLARE @DprIds_4_1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_4_1
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_4_1
		
	DECLARE @DprsWithForeignKeys_4_1 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_4_1 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_4_1  as context
		INNER JOIN @DprIds_4_1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_4_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_4_1 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_4_1

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_4_1 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_4_1 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_4_1 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_4_1
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_4_1
	WHERE
		DatahandlerSupervisionLink IS NOT NULL



	/*
		Migration 2 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where all system GDPR data is the same.
	*/

	DECLARE @ContractMigrationData_4_2 TABLE
	(
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max)
	)

	INSERT INTO @ContractMigrationData_4_2
	SELECT
		ItContract.Id,
		ItContract.OrganizationId,
		GETUTCDATE(),
		ItContract.Name,
		ItContract.ContainsDataHandlerAgreement,
		ItContract.DataHandlerAgreementUrl,
		ItContract.DataHandlerAgreementUrlName,
		ItContract.Note,
		ItContract.ObjectOwnerId,
		ItContract.LastChangedByUserId,
		AllContractsWithEqualSystemGDPRData.dataProcessor,
		CASE
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 0 THEN 1
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl = 1 THEN 0
			WHEN AllContractsWithEqualSystemGDPRData.dataProcessorControl IS NULL THEN NULL
			ELSE 2
		END AS OversightConcluded,
		AllContractsWithEqualSystemGDPRData.lastControl,
		AllContractsWithEqualSystemGDPRData.noteUsage,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrl,
		AllContractsWithEqualSystemGDPRData.datahandlerSupervisionDocumentationUrlName
	FROM
		@AllContractsWithEqualSystemGDPRData_4 AS AllContractsWithEqualSystemGDPRData
		INNER JOIN
		ItContract ON ItContract.Id = AllContractsWithEqualSystemGDPRData.ItContractId


	DECLARE @DprIds_4_2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_4_2
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData_4_2
		
	DECLARE @DprsWithForeignKeys_4_2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_4_2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData_4_2  as context
		INNER JOIN @DprIds_4_2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_4_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_4_2 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprsWithForeignKeys.DprId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_4_2 AS DprsWithForeignKeys
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = DprsWithForeignKeys.ItContractId

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_4_2 AS dprsWithForeign
		INNER JOIN
		ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = dprsWithForeign.ItContractId
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON ItContractItSystemUsages.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_4_2 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_4_2 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_4_2
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_4_2
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Migration 3 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where system GDPR data is different.
	*/

	DECLARE @MigrationContext_4_3 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		ContractName varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractNote varchar(max),
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int,
		SystemUsageUserId int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext_4_3 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, ContractName, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			ItContract.Name + '_' + CAST(ItSystemUsage.Id as varchar(max)),
			ItContract.ContainsDataHandlerAgreement,
			ItContract.DataHandlerAgreementUrl,
			ItContract.DataHandlerAgreementUrlName,
			ItContract.Note,
			ItSystemUsage.dataProcessor,
			CASE
				WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
				WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
				WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
				ELSE 2
			END AS OversightConcluded,
			ItSystemUsage.lastControl,
			ItSystemUsage.noteUsage,
			ItSystemUsage.datahandlerSupervisionDocumentationUrl,
			ItSystemUsage.datahandlerSupervisionDocumentationUrlName,
			ItContract.ObjectOwnerId,
			ItContract.LastChangedByUserId,
			ItSystemUsage.ObjectOwnerId
        FROM 
			@ContractIdsWithUnequalSystemGDPRData_4 AS ContractIdsWithUnequalSystemGDPRData
			INNER JOIN
			ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractIdsWithUnequalSystemGDPRData.ItContractId
			INNER JOIN 
			ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId 
			INNER JOIN 
			ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId


	DECLARE @DprIds_4_3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_4_3
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext_4_3 


	DECLARE @DprsWithForeignKeys_4_3 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys_4_3 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext_4_3  as context
		INNER JOIN @DprIds_4_3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys_4_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON Id = dprsWithForeign.ItContractId
	WHERE 
		ParentId IS NOT NULL

	/*
		Create contract children -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, Id
	FROM 
		@DprsWithForeignKeys_4_3 AS dprsWithForeign
		INNER JOIN
		ItContract ON ParentId = dprsWithForeign.ItContractId

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys_4_3

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys_4_3 AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Update contract advices to DPR 
	*/

	UPDATE
		Advice
	SET
		RelationId = dprsWithForeign.dprId,
		Type = 4
	FROM 
		Advice
		INNER JOIN
		@DprsWithForeignKeys_4_3 AS dprsWithForeign ON Advice.RelationId = dprsWithForeign.ItContractId
	WHERE Type = 0 

	/*
		Copy contract refernces to DPR
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, LastChangedByUserId, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		Title,
		URL,
		Display,
		LastChanged,
		LastChangedByUserId,
		Created,
		dprId,
		ObjectOwnerId
	FROM 
		@DprsWithForeignKeys_4_3 AS dprsWithForeign
		INNER JOIN
		ExternalReferences ON ExternalReferences.ItContract_Id = dprsWithForeign.ItContractId

	/*
		Create references if data handler agreement contains a url
		(Name should be "Link til databehandleraftale")
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN AgreementConcludedUrlName IS NULL THEN 'Link til databehandleraftale'
			ELSE AgreementConcludedUrlName 
		END AS Title,
		AgreementConcludedUrl, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		contractOwnerId
	FROM 
		@DprsWithForeignKeys_4_3
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		systemOwnerId
	FROM 
		@DprsWithForeignKeys_4_3
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


END

/*
	Fifth migration situation
	Systems with GDPR data and not associated with contracts
*/

BEGIN

	DECLARE @MigrationContext_5 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		Name varchar(max),
		DataResponsibleNote varchar(max),
		OversightConcluded int,
		OversightConcludedDate varchar(max),
		OversighConcludedRemark varchar(max),
		DatahandlerSupervisionLink varchar(max),
		DatahandlerSupervisionLinkName varchar(max),
		SystemUsageLastChangedBy int,
		SystemUsageUserId int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext_5 (ItSystemUsageId, OrganizationId, LastChanged, Name, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, SystemUsageLastChangedBy, SystemUsageUserId)
        SELECT
			ItSystemUsage.Id,
			ItSystemUsage.OrganizationId,
			GETUTCDATE(),
			ItSystem.Name,
			ItSystemUsage.dataProcessor,
			CASE
				WHEN ItSystemUsage.dataProcessorControl = 0 THEN 1
				WHEN ItSystemUsage.dataProcessorControl = 1 THEN 0
				WHEN ItSystemUsage.dataProcessorControl IS NULL THEN NULL
				ELSE 2
			END AS OversightConcluded,
			ItSystemUsage.lastControl,
			ItSystemUsage.noteUsage,
			ItSystemUsage.datahandlerSupervisionDocumentationUrl,
			ItSystemUsage.datahandlerSupervisionDocumentationUrlName,
			ItSystemUsage.LastChangedByUserId,
			ItSystemUsage.ObjectOwnerId
        FROM 
			kitos.dbo.ItSystemUsage
			INNER JOIN
			kitos.dbo.ItSystem ON ItSystem.Id = ItSystemUsage.ItSystemId
		WHERE 
			ItSystemUsage.Id NOT IN (
				SELECT
					ItSystemUsageId
				FROM
					kitos.dbo.ItContractItSystemUsages
					INNER JOIN
					kitos.dbo.ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId
			)
			AND
			ItSystemUsage.Id NOT IN (
				SELECT
					ItSystemUsage_Id
				FROM
					kitos.dbo.DataProcessingRegistrationItSystemUsages
			)
			AND 
			(
				ItSystemUsage.dataProcessor IS NOT NULL
				OR
				(
					ItSystemUsage.dataProcessorControl IS NOT NULL
					AND
					ItSystemUsage.dataProcessorControl != 0
				)
				OR
				ItSystemUsage.lastControl IS NOT NULL
				OR
				ItSystemUsage.noteUsage IS NOT NULL
				OR
				ItSystemUsage.datahandlerSupervisionDocumentationUrl IS NOT NULL
				OR
				ItSystemUsage.Id IN (
					SELECT
						ItSystemUsageId
					FROM
						kitos.dbo.ItSystemUsageDataWorkerRelations
				)
			)
				

	DECLARE @DprIds_5 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		kitos.dbo.DataProcessingRegistrations (OrganizationId, Name, LastChanged, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds_5
	SELECT
		OrganizationId, Name, LastChanged, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, SystemUsageUserId, SystemUsageLastChangedBy
	FROM 
		@MigrationContext_5


	DECLARE @DprsWithSystemKeys_5 table (dprId int, ItSystemUsageId int, DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), systemOwnerId int)

	/*
		Create temp table with dprId's and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithSystemKeys_5 (dprId, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, systemOwnerId)
	SELECT
		id, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, SystemUsageUserId
	FROM 
		@MigrationContext_5  as context
		INNER JOIN @DprIds_5 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract DPR relations
	*/

	INSERT INTO
		kitos.dbo.DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithSystemKeys_5

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		kitos.dbo.DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithSystemKeys_5 AS dprWithSystem
		INNER JOIN
		kitos.dbo.ItSystemUsageDataWorkerRelations ON dprWithSystem.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL
		AND
		DataWorkerId NOT IN (
			SELECT Organization_Id
			FROM kitos.dbo.DataProcessingRegistrationOrganizations
			WHERE DataProcessingRegistration_Id = DprId
		)

	/*
		Create references if systemusage contains "Link til dokumentation"
		(Name should be "Link til tilsynsdokumentation" if not provided by the link)
	*/

	INSERT INTO
		kitos.dbo.ExternalReferences (Title, URL, Display, LastChanged, Created, DataProcessingRegistration_Id, ObjectOwnerId)
	SELECT
		CASE 
			WHEN DatahandlerSupervisionLinkName IS NULL THEN 'Link til tilsynsdokumentation'
			ELSE DatahandlerSupervisionLinkName 
		END AS Title, 
		DatahandlerSupervisionLink, 
		0, 
		GETUTCDATE(), 
		GETUTCDATE(), 
		dprId,
		systemOwnerId
	FROM 
		@DprsWithSystemKeys_5
	WHERE
		DatahandlerSupervisionLink IS NOT NULL

END 



/*
	Handle updating of affected contracts
*/

BEGIN
	
	/*
		Resets contract type and associate contract with DPR from migration situation 2
	*/

	UPDATE 
		ItContract
	SET
		ContractTypeId = null
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_2_1
	)

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_2_1


	UPDATE 
		ItContract
	SET
		ContractTypeId = null
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_2_2
	)

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_2_2


	UPDATE 
		ItContract
	SET
		ContractTypeId = null
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys_2_3
	)

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_2_3



	
	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_4_1


	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_4_2

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys_4_3

END

/* 
	Adding dpr's to pending readmodel updates so readmodel is updated
*/
BEGIN
	INSERT INTO
		PendingReadModelUpdates (SourceId, Category, CreatedAt)
	SELECT
		id, 0, GETUTCDATE()
	FROM
		DataProcessingRegistrations
END