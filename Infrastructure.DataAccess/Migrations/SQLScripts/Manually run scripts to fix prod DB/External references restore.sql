/* KITOSUDV-1285 external references was not initially copied over */

/* Restore external references */
/* REMEMBER TO CHECK WHICH TABLES ARE SET */
UPDATE prodTable
SET prodTable.ExternalReferenceId = newMigrationTable.ExternalReferenceId
FROM kitosProd.dbo.ExternalReferences AS prodTable, kitos.dbo.ExternalReferences AS newMigrationTable
WHERE prodTable.Id = newMigrationTable.Id 
AND prodTable.DataProcessingRegistration_Id IS NOT NULL
AND prodTable.LastChanged <= '2020/11/23 22:55:35.5130000'
AND prodTable.ExternalReferenceId IS NULL
AND newMigrationTable.ExternalReferenceId IS NOT NULL