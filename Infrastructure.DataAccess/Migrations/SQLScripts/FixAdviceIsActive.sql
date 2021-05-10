/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1310
 
Content:
    Updates existing IsActive column on Advice, so that all past advices who still has IsActive=1 is hard set to IsActive=0

    Also, the related hangfire jobs are deleted, if any
*/

BEGIN
    DECLARE @MigrationContext TABLE
    (
        AdviceId int,
        JobId nvarchar(max)
    );

    INSERT INTO @MigrationContext
        SELECT 
            [Id] As AdviceId, 
            [JobId] As JobId
        FROM [kitos].[dbo].[Advice]
        WHERE IsActive=1 AND StopDate<=GETUTCDATE()

    UPDATE [kitos].[dbo].[Advice]
        SET IsActive=0
    WHERE IsActive=1 AND StopDate<=GETUTCDATE()

    DELETE FROM [kitos_HangfireDB].[Hangfire].[Set]
    WHERE EXISTS 
		(SELECT * 
		 FROM @MigrationContext 
		 WHERE JobId LIKE Value)
END