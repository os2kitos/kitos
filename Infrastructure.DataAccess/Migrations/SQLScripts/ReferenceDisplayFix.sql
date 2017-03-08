/* Resets */
UPDATE dbo.ItSystemUsage
SET ReferenceId = NULL

UPDATE dbo.ItSystem
SET ReferenceId = NULL

UPDATE dbo.ItContract
SET ReferenceId = NULL

UPDATE dbo.ExternalReferences
SET Display = 0

/* Sets the reference */
UPDATE S 
SET ReferenceId = R.Id
FROM dbo.ItSystem S
INNER JOIN dbo.ExternalReferences R
ON S.Id = R.ItSystem_Id
WHERE S.ReferenceId IS NULL AND R.URL IS NOT NULL

/* Sets the reference */
UPDATE S 
SET ReferenceId = R.Id
FROM dbo.ItSystemUsage S
INNER JOIN dbo.ExternalReferences R
ON S.Id = R.ItSystemUsage_Id
WHERE S.ReferenceId IS NULL AND R.URL IS NOT NULL

/* Sets the reference */
UPDATE S 
SET ReferenceId = R.Id
FROM dbo.ItContract S
INNER JOIN dbo.ExternalReferences R
ON S.Id = R.ItContract_Id
WHERE S.ReferenceId IS NULL AND R.URL IS NOT NULL


/* Sets display */
UPDATE R
SET Display = 1
FROM dbo.ExternalReferences R
INNER JOIN dbo.ItSystem S
ON S.Id = R.ItSystem_Id
WHERE R.Id = S.ReferenceId AND R.Url IS NOT NULL

/* Sets display */
UPDATE R
SET Display = 1
FROM dbo.ExternalReferences R
INNER JOIN dbo.ItSystemUsage S
ON S.Id = R.ItSystemUsage_Id
WHERE R.Id = S.ReferenceId AND R.Url IS NOT NULL

/* Sets display */
UPDATE R
SET Display = 1
FROM dbo.ExternalReferences R
INNER JOIN dbo.ItContract S
ON S.Id = R.ItContract_Id
WHERE R.Id = S.ReferenceId AND R.Url IS NOT NULL
