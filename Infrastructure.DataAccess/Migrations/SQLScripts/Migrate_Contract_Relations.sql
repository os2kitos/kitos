/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-499
 
Content:
 Migrates existing contract relations according to the following logic

 If an ItInterfaceUse points to an ItInterface for which the exposing system is in use in the organization, THEN the interface useage is migrated to a system relation 
 between the "Using" system usage and the usage of the "interface exhibitor". If data is migrated, the old data is removed.
*/

use Kitos;

/*
- Get all organizations for which itinterface usage has been created!
- for each organization, collect
*/



BEGIN
    -- Build an operation context - the from-to context needed to migrate the relation
    DECLARE @MigrationContext TABLE
    (
        OrganizationId as int null,
        FromSystemUsageId as int,
        ToSystemUsageId as int null, -- if null, we cannot migrate it and we bail out later
        ExposingSystemId as int null,
        ItInterfaceId as int,
        ItContractId as int,
        ObjectOwnerId as int,
        LastChanged as datetime,
        LastChangedByUserId as int
    );
    
    -- Scrape candidate data from
    INSERT INTO @MigrationContext
        SELECT
            NULL AS OrganizationId,
            ItSystemUsageId AS FromSystemUsageId,
            NULL AS ToSystemUsageId,
            NULL AS ExposingSystemId,
            ItInterfaceId,
            ItContractId,
            ObjectOwnerId,
            LastChanged,
            LastChangedByUserId
        FROM ItInterfaceUsage
        WHERE ItContractId IS NOT NULL; -- if null, the user has actively deleted the relation from the UI.. an operation which only "nulls" the contract on the created relation in the backend

    -- Patch Exposing system Id
    UPDATE @MigrationContext
        SET 
            ExposingSystemId = (SELECT top(1) ItSystemId FROM Exhibit WHERE Id = ItInterfaceId),
            OrganizationId = (SELECT top(1) OrganizationId FROM ItSystemUsage WHERE Id = FromSystemUsageId);

    -- Delete all where exposing system id is still NULL (the relation was invalid since no logical IT system was marked as exhibitor)
    DELETE from @MigrationContext
    where ExposingSystemId IS NULL;
    
    -- Patch "ToSystemUsageId" by binding to local system usage Id of the exposing system
    UPDATE @MigrationContext
        SET ToSystemUsageId = (SELECT top(1) Id FROM ItSystemUsage WHERE ItSystemId = ExposingSystemId AND OrganizationId = OrganizationId);

    -- Delete all all rows wgere ToSystemUsageId could not be derived - that is - where no local usage of the exposing system exists
    DELETE from @MigrationContext
    where ToSystemUsageId IS NULL;

    -- Insert system relations
    INSERT INTO SystemRelation
        SELECT
           FromSystemUsageId AS FromSystemUsageId
           ,ToSystemUsageId AS ToSystemUsageId
           ,NULL AS Description
           ,ObjectOwnerId AS ObjectOwnerId
           ,LastChanged AS LastChanged
           ,LastChangedByUserId AS LastChangedByUserId
           ,ItContractId AS AssociatedContractId
           ,ItInterfaceId AS RelationInterfaceId
           ,NULL AS UsageFrequencyId
           ,NULL AS Reference
           ,NEWID() AS Uuid
     FROM @MigrationContext;

    -- TODO: Delete source data by PK ([ItSystemUsageId] [ItSystemId] [ItInterfaceId])

    -- TODO: Should we add the "migrated to" relation Id?.. in that way we can reverse the opration... just requires an additional field an a type which we are going to nuke anyways
END
