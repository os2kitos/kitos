
/*
Content:
    Ensures that projects, dprs and contracts all have unique names within their organization.
    This task is spawned because new constraints are added to the tables to guarantee the invariant (was previously enforced by serializable transactions and application logic).
*/

BEGIN
	
	-- DPR
	DECLARE @RenameDataProcessingRegistrationsContext TABLE
    (
        DataProcessingRegistrationId int,
        ModifiedName varchar(MAX)
    );

	insert into @RenameDataProcessingRegistrationsContext
		select 
			[DataProcessingRegistrations].Id as DataProcessingRegistrationId 
			, CONCAT([DataProcessingRegistrations].Name,' (',ROW_NUMBER() OVER(partition by [DataProcessingRegistrations].OrganizationId,[DataProcessingRegistrations].Name order by [DataProcessingRegistrations].Id asc),')') AS ModifiedName

		from
			(SELECT	count(0) as DUPLICATES 
				,[OrganizationId] 
				,[Name] 
			FROM [DataProcessingRegistrations]
			group by [OrganizationId],[Name]) AS A
			inner join [DataProcessingRegistrations] 
			on A.Name = [DataProcessingRegistrations].Name and A.OrganizationId = [DataProcessingRegistrations].OrganizationId
			where A.DUPLICATES > 1

	 MERGE INTO [DataProcessingRegistrations]
        USING @RenameDataProcessingRegistrationsContext
            ON [@RenameDataProcessingRegistrationsContext].DataProcessingRegistrationId = [DataProcessingRegistrations].Id
		WHEN MATCHED THEN
			UPDATE
				SET [DataProcessingRegistrations].Name = [@RenameDataProcessingRegistrationsContext].ModifiedName;

	-- Contracts
	DECLARE @RenameItContractContext TABLE
    (
        ItContractId int,
        ModifiedName varchar(MAX)
    );

	insert into @RenameItContractContext
		select 
			[ItContract].Id as ItContractId 
			, CONCAT([ItContract].Name,' (',ROW_NUMBER() OVER(partition by [ItContract].OrganizationId,[ItContract].Name order by [ItContract].Id asc),')') AS ModifiedName

		from
			(SELECT	count(0) as DUPLICATES 
				,[OrganizationId] 
				,[Name] 
			FROM [ItContract]
			group by [OrganizationId],[Name]) AS A
			inner join [ItContract] 
			on A.Name = [ItContract].Name and A.OrganizationId = [ItContract].OrganizationId
			where A.DUPLICATES > 1

	 MERGE INTO [ItContract]
        USING @RenameItContractContext
            ON [@RenameItContractContext].ItContractId = [ItContract].Id
		WHEN MATCHED THEN
			UPDATE
				SET [ItContract].Name = [@RenameItContractContext].ModifiedName;

	-- Projects
	DECLARE @RenameItProjectContext TABLE
    (
        ItProjectId int,
        ModifiedName varchar(MAX)
    );

	insert into @RenameItProjectContext
		select 
			[ItProject].Id as ItProjectId 
			, CONCAT([ItProject].Name,' (',ROW_NUMBER() OVER(partition by [ItProject].OrganizationId,[ItProject].Name order by [ItProject].Id asc),')') AS ModifiedName

		from
			(SELECT	count(0) as DUPLICATES 
				,[OrganizationId] 
				,[Name] 
			FROM [ItProject]
			group by [OrganizationId],[Name]) AS A
			inner join [ItProject] 
			on A.Name = [ItProject].Name and A.OrganizationId = [ItProject].OrganizationId
			where A.DUPLICATES > 1

	 MERGE INTO [ItProject]
        USING @RenameItProjectContext
            ON [@RenameItProjectContext].ItProjectId = [ItProject].Id
		WHEN MATCHED THEN
			UPDATE
				SET [ItProject].Name = [@RenameItProjectContext].ModifiedName;

END