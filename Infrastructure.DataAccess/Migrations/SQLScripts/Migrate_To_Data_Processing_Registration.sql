/*

Content:
 Migrates existing system and contract GDPR data to Data Processing Registrations according to the logic described in https://os2web.atlassian.net/browse/KITOSUDV-1161

*/


/*
	First migration situation
	Contract with ContractType as "Databehandleraftale" (5) and 1 to many associated it system usages
*/

BEGIN

	DECLARE @MigrationContext1 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
		ItContractId int,
		ItSystemUsageId int,
		OrganizationId int,
		LastChanged datetime2(7),
		Name varchar(max),
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

	INSERT INTO @MigrationContext1 (ItContractId, ItSystemUsageId, OrganizationId, LastChanged, Name, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, ContractUserId, ContractLastChangedBy, SystemUsageUserId)
        SELECT
			ItContractItSystemUsages.ItContractId, 
			ItContractItSystemUsages.ItSystemUsageId,
			ItContract.OrganizationId,
			GETUTCDATE(),
			'Konverteret_Databehandling_',
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
		ItContractItSystemUsages 
			INNER JOIN ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId
			INNER JOIN ItSystemUsage ON ItSystemUsage.Id = ItContractItSystemUsages.ItSystemUsageId
		WHERE 
			ItContractItSystemUsages.ItContractId 
				IN (
					SELECT 
						Id 
					FROM 
						ItContract 
					WHERE 
						ContractTypeId = 5
				)

	DECLARE @DprIds1 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds1
	SELECT
		OrganizationId, Name, LastChanged, AgreementConcluded, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext1 


	DECLARE @DprsWithForeignKeys table (dprId int, ItContractId int, ItSystemUsageId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), contractOwnerId int, systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithForeignKeys (dprId, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, contractOwnerId, systemOwnerId)
	SELECT
		id, ItContractId, ItSystemUsageId, AgreementConcludedUrlName, AgreementConcludedUrl, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, ContractUserId, SystemUsageUserId
	FROM 
		@MigrationContext1  as context
		INNER JOIN @DprIds1 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithForeignKeys

	/*
		Create system DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithForeignKeys

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT
		DprId, DataWorkerId
	FROM 
		@DprsWithForeignKeys AS dprsWithForeign
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprsWithForeign.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL


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
		@DprsWithForeignKeys
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
		@DprsWithForeignKeys
	WHERE
		DatahandlerSupervisionLink IS NOT NULL



	/* 
		Adding dpr's to pending readmodel updates so readmodel is updated
	*/
	INSERT INTO
		PendingReadModelUpdates (SourceId, Category, CreatedAt)
	SELECT
		id, 0, GETUTCDATE()
	FROM
		@DprIds1

END

/*
	Second migration situation
	Contract with ContractType as "Databehandleraftale" (5) and 0 associated it system usages
*/

BEGIN

	DECLARE @MigrationContext2 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
        ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		Name varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext2 (ItContractId, OrganizationId, LastChanged, Name, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractUserId, ContractLastChangedBy)
        SELECT
			Id, 
			OrganizationId,
			GETUTCDATE(),
			'Konverteret_Databehandling_',
			ContainsDataHandlerAgreement,
			DataHandlerAgreementUrl,
			DataHandlerAgreementUrlName,
			ObjectOwnerId,
			LastChangedByUserId
        FROM 
			ItContract
		WHERE 
			ContractTypeId = 5
			AND
			Id NOT IN (
				SELECT
					ItContractId
				FROM
					ItContractItSystemUsages
			)
				

	DECLARE @DprIds2 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds2
	SELECT
		OrganizationId, Name, LastChanged, AgreementConcluded, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext2 


	DECLARE @DprsWithContractKeys table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithContractKeys (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, ContractUserId
	FROM 
		@MigrationContext2  as context
		INNER JOIN @DprIds2 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithContractKeys


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
		@DprsWithContractKeys
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/* 
		Adding dpr's to pending readmodel updates so readmodel is updated
	*/

	INSERT INTO
		PendingReadModelUpdates (SourceId, Category, CreatedAt)
	SELECT
		id, 0, GETUTCDATE()
	FROM
		@DprIds2

END

/*
	Third migration situation
	Contract with ContractType NOT as "Databehandleraftale" (5) and "Er der lavet en databehandleraftale" set to "Ja" (1) (It system usages are ignored here)
*/

BEGIN

	DECLARE @MigrationContext3 TABLE 
    (
		rowNumber int IDENTITY(1,1) PRIMARY KEY,
        ItContractId int,
		OrganizationId int,
		LastChanged datetime2(7),
		Name varchar(max),
		AgreementConcluded int, 
		AgreementConcludedUrl varchar(max),
		AgreementConcludedUrlName varchar(max),
		ContractUserId int,
		ContractLastChangedBy int
    )
	

	/*
		Get all info needed for first migration situation
	*/

	INSERT INTO @MigrationContext3 (ItContractId, OrganizationId, LastChanged, Name, AgreementConcluded, AgreementConcludedUrl, AgreementConcludedUrlName, ContractUserId, ContractLastChangedBy)
        SELECT
			Id, 
			OrganizationId,
			GETUTCDATE(),
			'Konverteret_Databehandling_',
			ContainsDataHandlerAgreement,
			DataHandlerAgreementUrl,
			DataHandlerAgreementUrlName,
			ObjectOwnerId,
			LastChangedByUserId
        FROM 
			ItContract
		WHERE 
			(
				ContractTypeId IS NULL
				OR
				ContractTypeId <> 5
			)
			AND
			ContainsDataHandlerAgreement = 1
				

	DECLARE @DprIds3 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, IsAgreementConcluded, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds3
	SELECT
		OrganizationId, Name, LastChanged, AgreementConcluded, ContractUserId, ContractLastChangedBy
	FROM 
		@MigrationContext3 


	DECLARE @DprsWithContractKeys2 table (dprId int, ItContractId int, AgreementConcludedUrlName varchar(max), AgreementConcludedUrl varchar(max), contractOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithContractKeys2 (dprId, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, contractOwnerId)
	SELECT
		id, ItContractId, AgreementConcludedUrlName, AgreementConcludedUrl, ContractUserId
	FROM 
		@MigrationContext3  as context
		INNER JOIN @DprIds3 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract DPR relations
	*/

	INSERT INTO
		ItContractDataProcessingRegistrations (DataProcessingRegistration_Id, ItContract_Id)
	SELECT
		DprId, ItContractId
	FROM 
		@DprsWithContractKeys2


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
		@DprsWithContractKeys2
	WHERE
		AgreementConcludedUrl IS NOT NULL

	/* 
		Adding dpr's to pending readmodel updates so readmodel is updated
	*/

	INSERT INTO
		PendingReadModelUpdates (SourceId, Category, CreatedAt)
	SELECT
		id, 0, GETUTCDATE()
	FROM
		@DprIds3

END

/*
	Fourth migration situation
	It system which is not associated with a contract that is a "Databehandleraftale", but where the system contains data which will be deleted by migration
*/

BEGIN

	DECLARE @MigrationContext4 TABLE 
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

	INSERT INTO @MigrationContext4 (ItSystemUsageId, OrganizationId, LastChanged, Name, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, SystemUsageLastChangedBy, SystemUsageUserId)
        SELECT
			ItSystemUsage.Id,
			ItSystemUsage.OrganizationId,
			GETUTCDATE(),
			'Konverteret_Databehandling_',
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
			ItSystemUsage
		WHERE 
			ItSystemUsage.Id NOT IN (
				SELECT
					ItSystemUsageId
				FROM
					ItContractItSystemUsages
					INNER JOIN
					ItContract ON ItContract.Id = ItContractItSystemUsages.ItContractId
				WHERE
					ItContract.ContractTypeId = 5
			)
			AND 
			(
				ItSystemUsage.dataProcessor IS NOT NULL
				OR
				ItSystemUsage.dataProcessorControl IS NOT NULL
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
						ItSystemUsageDataWorkerRelations
				)
			)
				

	DECLARE @DprIds4 table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds4
	SELECT
		OrganizationId, Name, LastChanged, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, SystemUsageUserId, SystemUsageLastChangedBy
	FROM 
		@MigrationContext4 


	DECLARE @DprsWithSystemKeys table (dprId int, ItSystemUsageId int, DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), systemOwnerId int)

	/*
		Create temp table with dprId's and Contract and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithSystemKeys (dprId, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, systemOwnerId)
	SELECT
		id, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, SystemUsageUserId
	FROM 
		@MigrationContext4  as context
		INNER JOIN @DprIds4 as dprs ON context.rowNumber = dprs.rowNumber

	/*
		Create contract DPR relations
	*/

	INSERT INTO
		DataProcessingRegistrationItSystemUsages (DataProcessingRegistration_Id, ItSystemUsage_Id)
	SELECT
		DprId, ItSystemUsageId
	FROM 
		@DprsWithSystemKeys

	/*
		Create DPR Data Worker
	*/

	INSERT INTO
		DataProcessingRegistrationOrganizations (DataProcessingRegistration_Id, Organization_Id)
	SELECT
		DprId, DataWorkerId
	FROM 
		@DprsWithSystemKeys AS dprWithSystem
		INNER JOIN
		ItSystemUsageDataWorkerRelations ON dprWithSystem.ItSystemUsageId = ItSystemUsageDataWorkerRelations.ItSystemUsageId
	WHERE DataWorkerId IS NOT NULL

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
		@DprsWithSystemKeys
	WHERE
		DatahandlerSupervisionLink IS NOT NULL

	/* 
		Adding dpr's to pending readmodel updates so readmodel is updated
	*/

	INSERT INTO
		PendingReadModelUpdates (SourceId, Category, CreatedAt)
	SELECT
		id, 0, GETUTCDATE()
	FROM
		@DprIds4

END


BEGIN

	/*
		Reset contract type on affected contracts
	*/

	UPDATE ItContract
	SET ContractTypeId = NULL
	WHERE ContractTypeId = 5

	/*
		Add the Id of the DPR to the name
	*/

	UPDATE DataProcessingRegistrations
	SET Name = Name + CAST(Id as varchar(max))
	WHERE Name = 'Konverteret_Databehandling_'

END
