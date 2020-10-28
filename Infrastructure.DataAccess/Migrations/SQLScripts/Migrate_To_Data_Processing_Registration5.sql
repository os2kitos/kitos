/*

Content:
 Migrates existing system and contract GDPR data to Data Processing Registrations according to the logic described in https://os2web.atlassian.net/browse/KITOSUDV-1271

*/

/*
	Fifth migration situation
	Systems with GDPR data and not associated with contracts
*/

BEGIN

	DECLARE @MigrationContext TABLE 
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

	INSERT INTO @MigrationContext (ItSystemUsageId, OrganizationId, LastChanged, Name, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, DatahandlerSupervisionLink, DatahandlerSupervisionLinkName, SystemUsageLastChangedBy, SystemUsageUserId)
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
			ItSystemUsage
			INNER JOIN
			ItSystem ON ItSystem.Id = ItSystemUsage.ItSystemId
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
				

	DECLARE @DprIds table (rowNumber int IDENTITY(1,1) PRIMARY KEY, id int)

	/*
		Create new DPR's and save created Id's in a new table
	*/

	INSERT INTO 
		DataProcessingRegistrations (OrganizationId, Name, LastChanged, DataResponsibleRemark, IsOversightCompleted, LatestOversightDate, OversightCompletedRemark, ObjectOwnerId, LastChangedByUserId)
	OUTPUT
		inserted.Id into @DprIds
	SELECT
		OrganizationId, Name, LastChanged, DataResponsibleNote, OversightConcluded, OversightConcludedDate, OversighConcludedRemark, SystemUsageUserId, SystemUsageLastChangedBy
	FROM 
		@MigrationContext 


	DECLARE @DprsWithSystemKeys table (dprId int, ItSystemUsageId int, DatahandlerSupervisionLinkName varchar(max), DatahandlerSupervisionLink varchar(max), systemOwnerId int)

	/*
		Create temp table with dprId's and SystemUsage Id's to be used when creating relations
	*/

	INSERT INTO 
		@DprsWithSystemKeys (dprId, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, systemOwnerId)
	SELECT
		id, ItSystemUsageId, DatahandlerSupervisionLinkName, DatahandlerSupervisionLink, SystemUsageUserId
	FROM 
		@MigrationContext  as context
		INNER JOIN @DprIds as dprs ON context.rowNumber = dprs.rowNumber

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

END 