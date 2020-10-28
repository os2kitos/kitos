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