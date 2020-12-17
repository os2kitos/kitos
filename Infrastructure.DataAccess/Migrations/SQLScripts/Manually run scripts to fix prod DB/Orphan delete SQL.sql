/* DELETE old orphan data */
DELETE
FROM [kitosProd].[dbo].[LocalRelationFrequencyTypes]
WHERE LastChanged <= '2018/01/01 01:00:00 AM'