/*

Content:
 Migrates existing system and contract GDPR data to Data Processing Registrations according to the logic described in https://os2web.atlassian.net/browse/KITOSUDV-1271

*/
/*
	Fourth migration situation
	Contract with ContractType as NOT "Databehandleraftale" (5) and DPR related 0 + other data on Contract
*/

BEGIN

	DECLARE @ItContractIdsToBeDeleted TABLE 
    (
		ItContractId int
    )

	INSERT INTO @ItContractIdsToBeDeleted
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
		)

	DECLARE @ContractsWithAtLeast2SystemUsages TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractsWithAtLeast2SystemUsages
	SELECT ItContractItSystemUsages.ItContractId
	FROM @ItContractIdsToBeDeleted AS contractIdsToBeDeleted
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = contractIdsToBeDeleted.ItContractId
	GROUP BY ItContractItSystemUsages.ItContractId
	HAVING COUNT(*) > 1

	DECLARE @ContractsAndUsagesWith0Or1SystemUsages TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWith0Or1SystemUsages
	SELECT contractIdsToBeDeleted.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM 
		@ItContractIdsToBeDeleted AS contractIdsToBeDeleted
		LEFT JOIN
		ItContractItSystemUsages ON contractIdsToBeDeleted.ItContractId = ItContractItSystemUsages.ItContractId
	WHERE contractIdsToBeDeleted.ItContractId NOT IN 
		(SELECT ContractsWithAtLeast2SystemUsages.ItContractId
		FROM @ContractsWithAtLeast2SystemUsages AS ContractsWithAtLeast2SystemUsages
		)

	DECLARE @ContractsAndUsagesWithAtLeast2SystemUsages TABLE
	(
		ItContractId int,
		ItSystemUsageId int
	)

	INSERT INTO @ContractsAndUsagesWithAtLeast2SystemUsages
	SELECT ItContractItSystemUsages.ItContractId, ItContractItSystemUsages.ItSystemUsageId
	FROM @ContractsWithAtLeast2SystemUsages AS ContractsWithAtLeast2SystemUsages
		INNER JOIN ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractsWithAtLeast2SystemUsages.ItContractId

	DECLARE @AllContractsWithMultipleSystemUsagesGrouped TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithMultipleSystemUsagesGrouped
	SELECT 
		ItContractId,
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName
	FROM 
		@ContractsAndUsagesWithAtLeast2SystemUsages
		INNER JOIN 
		ItSystemUsage ON Id = ItSystemUsageId
	GROUP BY ItContractId, 
		ItSystemUsage.dataProcessor,
		ItSystemUsage.dataProcessorControl,
		ItSystemUsage.lastControl,
		ItSystemUsage.noteUsage,
		ItSystemUsage.datahandlerSupervisionDocumentationUrl,
		ItSystemUsage.datahandlerSupervisionDocumentationUrlName


	DECLARE @ContractIdWhereSystemUsagesDataIsEqual TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual
	SELECT
		ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped
	GROUP BY 
		ItContractId
	HAVING
		COUNT(*) < 2


	INSERT INTO @ContractIdWhereSystemUsagesDataIsEqual
	SELECT ItContractId
	FROM @AllContractsWithMultipleSystemUsagesGrouped
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual
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


	DECLARE @AllContractsWithEqualSystemGDPRData TABLE 
	(
		ItContractId int,
		dataProcessor varchar(max),
		dataProcessorControl varchar(max),
		lastControl varchar(max),
		noteUsage varchar(max),
		datahandlerSupervisionDocumentationUrl varchar(max),
		datahandlerSupervisionDocumentationUrlName varchar(max)
	)

	INSERT INTO @AllContractsWithEqualSystemGDPRData
	SELECT *
	FROM @AllContractsWithMultipleSystemUsagesGrouped
	WHERE 
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual
		)
		AND
		(
			ItContractId NOT IN (
					SELECT ItContractId
					FROM @AllContractsWithMultipleSystemUsagesGrouped
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

	INSERT INTO @AllContractsWithEqualSystemGDPRData
	SELECT ItContractId, MAX(dataProcessor), MIN(dataProcessorControl), MAX(lastControl), MAX(noteUsage), MAX(datahandlerSupervisionDocumentationUrl), MAX(datahandlerSupervisionDocumentationUrlName)
	FROM @AllContractsWithMultipleSystemUsagesGrouped
	WHERE
		ItContractId IN (
			SELECT ItContractId
			FROM @ContractIdWhereSystemUsagesDataIsEqual
		)
		AND
		ItContractId NOT IN (
			SELECT ItContractId
			FROM @AllContractsWithEqualSystemGDPRData
		)
	GROUP BY ItContractId

	DECLARE @ContractIdsWithUnequalSystemGDPRData TABLE
	(
		ItContractId int
	)

	INSERT INTO @ContractIdsWithUnequalSystemGDPRData
	SELECT
		ItContractId
	FROM 
		@ContractsWithAtLeast2SystemUsages
	WHERE ItContractId NOT IN(
		SELECT
			ItContractId
		FROM 
			@ContractIdWhereSystemUsagesDataIsEqual
		)

	/*
		Migration 1 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 0 or 1 It System Usages associated.
	*/

	DECLARE @ContractMigrationData1 TABLE
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

	INSERT INTO @ContractMigrationData1
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
		@ContractsAndUsagesWith0Or1SystemUsages AS ContractsWith0Or1SystemUsages
		INNER JOIN
		ItContract ON ItContract.Id = ContractsWith0Or1SystemUsages.ItContractId
		INNER JOIN
		ItSystemUsage ON ItSystemUsage.Id = ContractsWith0Or1SystemUsages.ItSystemUsageId


	DECLARE @DprIds1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds1
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData1
		
	DECLARE @DprsWithForeignKeys1 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys1 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData1  as context
		INNER JOIN @DprIds1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys1 AS dprsWithForeign
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
		@DprsWithForeignKeys1 AS dprsWithForeign
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
		@DprsWithForeignKeys1

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys1 AS dprsWithForeign
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
		Copy contract advices to DPR
	*/

	INSERT INTO
		Advice (IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, RelationId, Type, Scheduling, StopDate, Body, JobId)
	SELECT
		IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, dprsWithForeign.dprId, 4, Scheduling, StopDate, Body, JobId
	FROM 
		@DprsWithForeignKeys1 AS dprsWithForeign
		INNER JOIN
		Advice ON Advice.RelationId = dprsWithForeign.ItContractId
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
		@DprsWithForeignKeys1 AS dprsWithForeign
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
		@DprsWithForeignKeys1
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
		@DprsWithForeignKeys1
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Resets contract type and associate contract with DPR
	*/

	UPDATE 
		ItContract
	SET
		ContractTypeId = null
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys1
	)

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys1


	/*
		Migration 2 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where all system GDPR data is the same.
	*/

	DECLARE @ContractMigrationData2 TABLE
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

	INSERT INTO @ContractMigrationData2
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
		@AllContractsWithEqualSystemGDPRData AS AllContractsWithEqualSystemGDPRData
		INNER JOIN
		ItContract ON ItContract.Id = AllContractsWithEqualSystemGDPRData.ItContractId


	DECLARE @DprIds2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, AgreementConcludedRemark, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds2
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, ContractNote, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@ContractMigrationData2
		
	DECLARE @DprsWithForeignKeys2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId
	FROM 
		@ContractMigrationData2  as context
		INNER JOIN @DprIds2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys2 AS dprsWithForeign
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
		@DprsWithForeignKeys2 AS dprsWithForeign
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
		@DprsWithForeignKeys2 AS DprsWithForeignKeys
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
		@DprsWithForeignKeys2 AS dprsWithForeign
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
		Copy contract advices to DPR
	*/

	INSERT INTO
		Advice (IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, RelationId, Type, Scheduling, StopDate, Body, JobId)
	SELECT
		IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, dprsWithForeign.dprId, 4, Scheduling, StopDate, Body, JobId
	FROM 
		@DprsWithForeignKeys2 AS dprsWithForeign
		INNER JOIN
		Advice ON Advice.RelationId = dprsWithForeign.ItContractId
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
		@DprsWithForeignKeys2 AS dprsWithForeign
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
		@DprsWithForeignKeys2
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
		@DprsWithForeignKeys2
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Resets contract type and associate contract with DPR
	*/

	UPDATE 
		ItContract
	SET
		ContractTypeId = null
	WHERE 
		Id IN (
		SELECT ItContractId
		FROM @DprsWithForeignKeys2
	)

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys2

	/*
		Migration 3 - Contract with "Databehandleraftale" type and only DPR related data inserted. With 2 or more It System Usages associated where system GDPR data is different.
	*/

	DECLARE @MigrationContext3 TABLE 
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

	INSERT INTO @MigrationContext3 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, ContractName, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			ItContract.Name + '_' + CAST(ItSystemUsage.Id as varchar(max)),
			ItContract.ContainsDataHandlerAgreement,
			ItContract.DataHandlerAgreementUrl,
			ItContract.DataHandlerAgreementUrlName,
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
			@ContractIdsWithUnequalSystemGDPRData AS ContractIdsWithUnequalSystemGDPRData
			INNER JOIN
			ItContractItSystemUsages ON ItContractItSystemUsages.ItContractId = ContractIdsWithUnequalSystemGDPRData.ItContractId
			INNER JOIN 
			ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId 
			INNER JOIN 
			ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId


	DECLARE @DprIds3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds3
	SELECT
		OrganizationId, ContractName, LastChanged, AgreementConcluded, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext3 


	DECLARE @DprsWithForeignKeys3 table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys3 (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext3  as context
		INNER JOIN @DprIds3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract parent -> DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ParentId
	FROM 
		@DprsWithForeignKeys3 AS dprsWithForeign
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
		@DprsWithForeignKeys3 AS dprsWithForeign
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
		@DprsWithForeignKeys3

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT DISTINCT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys3 AS dprsWithForeign
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
		Copy contract advices to DPR
	*/

	INSERT INTO
		Advice (IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, RelationId, Type, Scheduling, StopDate, Body, JobId)
	SELECT
		IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, dprsWithForeign.dprId, 4, Scheduling, StopDate, Body, JobId
	FROM 
		@DprsWithForeignKeys3 AS dprsWithForeign
		INNER JOIN
		Advice ON Advice.RelationId = dprsWithForeign.ItContractId
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
		@DprsWithForeignKeys3 AS dprsWithForeign
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
		@DprsWithForeignKeys3
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
		@DprsWithForeignKeys3
	WHERE
		DatahandlerSupervisionLink IS NOT NULL


	/*
		Associate contract with DPR
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys3

END
