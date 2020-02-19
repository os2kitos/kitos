/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-602
 
Content:
    Migrates old data from the "Url" field to a new external reference by the following logic:

    If there are already references associated with the system, no data is migrated.
    If there is valid data in the field and no existing references, the date is copied to a new reference associated with the system 
    and afterwards the new reference is set as the "master" reference on the system.
*/

BEGIN
    DECLARE @MigrationContext TABLE
    (
        ItSystemId int,
        OldUrlData varchar(MAX),
        ObjectOwnerId int
    );

    -- MIGRATE FROM URL TO REFERENCES
    INSERT INTO @MigrationContext
        SELECT
            Id            AS ItSystemId,
            [Url]         AS OldUrlData,
            ObjectOwnerId AS ObjectOwnerId
        FROM ItSystem
        WHERE 
            [Url] IS NOT NULL AND
            [Url] <> '' AND
            Id NOT IN (select ItSystem_Id AS Id from ExternalReferences where ItSystem_Id IS NOT NULL);

    INSERT INTO ExternalReferences
		(Title, 
		ExternalReferenceId,
		[Url],
		LastChanged,
		Created,
		ObjectOwnerId,
		LastChangedByUserId,
		Display,
		ItSystem_Id)
        SELECT
            'Reference'     AS Title,
            'Reference'     AS ExternalReferenceId,
            OldUrlData      AS [Url],
            GETUTCDATE()    AS LastChanged,
            GETUTCDATE()    AS Created,
            ObjectOwnerId   AS ObjectOwnerId,
            ObjectOwnerId   AS LastChangedByUserId,
            2               AS Display,
            ItSystemId      AS ItSystem_Id
        FROM @MigrationContext;

    -- SET MASTER REFERENCE
    DECLARE @SetMasterReferenceContext TABLE
    (
        ItSystemId int,
        ReferenceId int
    );
    
    INSERT INTO @SetMasterReferenceContext
        SELECT
            ItSystem.Id             AS ItSystemId,
            ExternalReferences.Id   AS ReferenceId
        FROM ItSystem
        INNER JOIN ExternalReferences ON
            ExternalReferences.ItSystem_Id = ItSystem.Id
        WHERE 
            ItSystem.Id IN (SELECT ItSystemId AS Id FROM @MigrationContext);

    -- Patch a reference to where the data was migrated to in the original table
    MERGE INTO ItSystem
        USING @SetMasterReferenceContext
            ON  [@SetMasterReferenceContext].ItSystemId = ItSystem.Id
    WHEN MATCHED THEN
        UPDATE
            SET [ItSystem].ReferenceId = [@SetMasterReferenceContext].ReferenceId;
END