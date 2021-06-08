/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1310
 
Content:
    Updates existing IsActive column on Advice, so that:
    - All past advices who still has IsActive=1 is hard set to IsActive=0
    - All future advices who has IsActive=0 is hard set to IsActive=1

    Also, the no longer active advices related hangfire jobs are deleted, if any
*/

BEGIN
    /* Past advice patch */
    DECLARE @MigrationContext TABLE
    (
        AdviceId int,
        JobId nvarchar(max)
    );

    INSERT INTO @MigrationContext
        SELECT 
            [Id] As AdviceId, 
            [JobId] As JobId
        FROM [Advice]
        WHERE IsActive=1 AND StopDate<=GETUTCDATE()

    UPDATE [Advice]
        SET IsActive=0
    FROM @MigrationContext WHERE Id=AdviceId

    /* Note: Check to see if database and table exists so that script succeeds on fresh install */
    IF DB_ID('kitos_HangfireDB') IS NOT NULL AND OBJECT_ID('Set', 'U') IS NOT NULL
        DELETE FROM [kitos_HangfireDB].[Hangfire].[Set]
        WHERE EXISTS 
		    (SELECT * 
		     FROM @MigrationContext 
		     WHERE JobId LIKE Value)

    /* Future advice patch */
    UPDATE [Advice]
        SET IsActive=1
    WHERE IsActive=0 AND StopDate>GETUTCDATE()
END