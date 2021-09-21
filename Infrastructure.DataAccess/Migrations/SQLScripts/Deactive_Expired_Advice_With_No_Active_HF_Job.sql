/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-2243
 
Content:
    Updates IsActive column on Advice, which should be deactivated when hangfire runs next send. But won't be as there is no hangfire job
*/

/* Note: Check to see if database and table exists so that script succeeds on fresh install */
IF DB_ID('kitos_HangfireDB') IS NOT NULL AND OBJECT_ID('Set', 'U') IS NOT NULL
BEGIN
	
	DECLARE @HFAdvice TABLE 
	(
		AdviceId int
	)

	INSERT INTO @HFAdvice
	SELECT CONVERT(int, SUBSTRING([Key], 23, 28)) AS AdviceId
	FROM [kitos_HangfireDB].[HangFire].[Hash]
	WHERE [Key] LIKE '%recurring-job:Advice: %'
	
	INSERT INTO @HFAdvice
	SELECT CONVERT(int, REVERSE(SUBSTRING(REVERSE(SUBSTRING([Arguments], 3, 8)), 3, 8))) AS AdviceId
	FROM [kitos_HangfireDB].[HangFire].[Job]
	WHERE [Arguments] NOT LIKE '%null%'

	DECLARE @IdsToDisable TABLE 
	(
		AdviceId int
	)

	INSERT INTO @IdsToDisable
	SELECT [Id]
	FROM [Kitos].[dbo].[Advice]
	WHERE Scheduling IS NOT NULL AND Scheduling != 0 AND StopDate < SYSUTCDATETIME() AND IsActive = 1
	AND [Id] NOT IN (
		SELECT *
		FROM @HFAdvice
	)

	UPDATE [Kitos].[dbo].[Advice]
	SET IsActive = 0
	WHERE Id IN (
		SELECT [AdviceId]
		FROM @IdsToDisable
	)
	
END