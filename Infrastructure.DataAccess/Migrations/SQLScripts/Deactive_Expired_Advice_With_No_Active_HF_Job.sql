/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-2243
 
Content:
	- Removes old Hangfire jobs which have no related data and should be deleted.
	- Updates IsActive column on Advice, which should be deactivated when Hangfire runs next send. But won't be as there is no Hangfire job
	- Recreates Hangfire jobs on Advices where there should be a job but isn't one.
*/

/* Note: Check to see if database and table exists so that script succeeds on fresh install */
IF DB_ID('kitos_HangfireDB') IS NOT NULL AND OBJECT_ID('Set', 'U') IS NOT NULL
BEGIN

    /* Part 1: Delete recurring jobs with no related data from hangfire DB to clean up*/
    DECLARE @RecurringJobsWithData TABLE 
    (
        AdviceId int
    )

    INSERT INTO @RecurringJobsWithData
    SELECT CONVERT(int, SUBSTRING([Key], 23, 28)) AS AdviceId
    FROM [kitos_HangfireDB].[HangFire].[Hash]
    WHERE [Key] LIKE '%recurring-job:Advice: %'

    INSERT INTO @RecurringJobsWithData
    SELECT CONVERT(int, REVERSE(SUBSTRING(REVERSE(SUBSTRING([Arguments], 3, 8)), 3, 8))) AS AdviceId
    FROM [kitos_HangfireDB].[HangFire].[Job]
    WHERE [Arguments] NOT LIKE '%null%'


    DECLARE @RecurringAdviceJobs TABLE 
    (
        AdviceId int,
        SetId int
    )

    INSERT INTO @RecurringAdviceJobs
    SELECT CONVERT(int, SUBSTRING([Value], 8, 6)), Id
    FROM [kitos_HangfireDB].[HangFire].[Set]
    WHERE [Key] LIKE 'recurring-jobs' AND [Score] > 0 AND [Value] LIKE 'Advice: %'

    DELETE
    FROM [kitos_HangfireDB].[HangFire].[Set] 
    WHERE Id IN (
        SELECT SetId
        FROM @RecurringAdviceJobs
        WHERE AdviceId NOT IN (
            SELECT * 
            FROM @RecurringJobsWithData
        )
    )


    /* Part 2: Find Id's of Advice which have related hangfire data */
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

    
	/* Part 3: Update Advice which are expired to be inactive */
	DECLARE @AdvicesToDisable TABLE 
	(
		AdviceId int
	)

	INSERT INTO @AdvicesToDisable
	SELECT [Id]
	FROM [Kitos].[dbo].[Advice]
	WHERE Scheduling IS NOT NULL AND Scheduling != 0 AND StopDate < GETUTCDATE() AND IsActive = 1
	AND [Id] NOT IN (
		SELECT *
		FROM @HFAdvice
	)
	
	UPDATE [Kitos].[dbo].[Advice]
	SET IsActive = 0
	WHERE Id IN (
		SELECT [AdviceId]
		FROM @AdvicesToDisable
	)
	

	/* Part 4: Locate active advice which have no related hangfire data */
	DECLARE @AdvicesToAddToHF TABLE 
	(
		AdviceId int,
		AlarmDate datetime2,
		Scheduling int
	)
	
	INSERT INTO @AdvicesToAddToHF
	SELECT [Id], [AlarmDate], [Scheduling]
	FROM [Kitos].[dbo].[Advice]
	WHERE Scheduling IS NOT NULL AND Scheduling != 0 AND StopDate >= GETUTCDATE() AND IsActive = 1
	AND [Id] NOT IN (
		SELECT *
		FROM @HFAdvice
	)

	/* Part 5: Add new hangfire jobs to set */
	DECLARE @IdsNotInHFSet TABLE 
	(
		AdviceId int
	)

	INSERT INTO @IdsNotInHFSet
	SELECT AdviceId
	FROM @AdvicesToAddToHF
	WHERE AdviceId NOT IN(
		SELECT CONVERT(int, SUBSTRING([Value], 8, 5))
		FROM [kitos_HangfireDB].[HangFire].[Set]
		WHERE [Key] LIKE 'recurring-jobs' AND [Score] > 0 AND [Value] LIKE 'Advice: %'
	)

	INSERT INTO [kitos_HangfireDB].[HangFire].[Set]
	(
		[Key],
		[Score],
		[Value],
		[ExpireAt]
	)
	SELECT
		'recurring-jobs',
		DATEDIFF(SECOND, DATEFROMPARTS(1970, 1, 1), SYSUTCDATETIME()),
		CONCAT('Advice: ', AdviceId),
		null
	FROM @IdsNotInHFSet
	

	/* Part 6: Fill Hash with data regarding the hangfire jobs */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Job', 
		CONCAT(CONCAT('{"Type":"Core.ApplicationServices.AdviceService, Core.ApplicationServices, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null","Method":"SendAdvice","ParameterTypes":"[\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"]","Arguments":"[\"' , AdviceId), '\"]"}'),
		NULL
	FROM @AdvicesToAddToHF


	/* Add advice with scheduling hourly */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Cron', 
		'0 * * * *',
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 1

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		(CONCAT(CONCAT(CONCAT(CONVERT(date, SYSUTCDATETIME()), 'T'), CONVERT(time, SYSUTCDATETIME())), 'Z')),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 1


	/* Add advice with scheduling daily */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Cron', 
		'0 8 * * *',
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 2

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 1, SYSUTCDATETIME()))), 'T08:00:00.0000000Z'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 2 AND AlarmDate <= SYSUTCDATETIME()

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CONCAT(DATEFROMPARTS(DATEPART(year, AlarmDate), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 2 AND AlarmDate > SYSUTCDATETIME()


	/* Add advice with scheduling weekly */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Cron', 
		CONCAT('0 8 * * ', CASE WHEN DATEPART(weekday, AlarmDate) = 1 THEN 7 ELSE DATEPART(weekday, AlarmDate) - 1 END), /* Hangfire understands 1 as monday while SQL understands 1 as sunday*/
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 3
	
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CASE 
			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 1, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 1, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')
			
			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 2, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 2, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')

			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 3, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 3, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')

			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 4, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 4, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')

			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 5, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 5, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')

			WHEN DATEPART(weekday, AlarmDate) = DATEPART(weekday, DATEADD(day, 6, SYSUTCDATETIME())) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 6, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')

			ELSE CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, DATEADD(day, 7, SYSUTCDATETIME()))), 'T08:00:00.0000000Z')
		END,
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 3 AND AlarmDate <= SYSUTCDATETIME()	

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CONCAT(DATEFROMPARTS(DATEPART(year, AlarmDate), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 3 AND AlarmDate > SYSUTCDATETIME()

	/* Add advice with scheduling monthly */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Cron', 
		CONCAT(CONCAT('0 8 ', DAY(AlarmDate)), ' * *'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 4
	
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CASE WHEN DATEPART(day, AlarmDate) >= DATEPART(day, SYSUTCDATETIME()) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, SYSUTCDATETIME()), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z')
			ELSE CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, DATEADD(month, 1, SYSUTCDATETIME())), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z')
		END,
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 4 AND AlarmDate <= SYSUTCDATETIME()	

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CONCAT(DATEFROMPARTS(DATEPART(year, AlarmDate), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 4 AND AlarmDate > SYSUTCDATETIME()
	

	/* Add advice with scheduling yearly */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Cron', 
		CONCAT(CONCAT(CONCAT(CONCAT('0 8 ', DAY(AlarmDate)), ' '), MONTH(AlarmDate)), ' *'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 5
	
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CASE WHEN DATEPART(month, AlarmDate) > DATEPART(month, SYSUTCDATETIME()) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z')
			
			WHEN DATEPART(month, AlarmDate) = DATEPART(month, SYSUTCDATETIME()) AND DATEPART(day, AlarmDate) > DATEPART(day, SYSUTCDATETIME()) 
			THEN CONCAT(DATEFROMPARTS(DATEPART(year, SYSUTCDATETIME()), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z')
			
			ELSE CONCAT(DATEFROMPARTS(DATEPART(year, DATEADD(year, 1, SYSUTCDATETIME())), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z')
		END,
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 5 AND AlarmDate <= SYSUTCDATETIME()	

	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'NextExecution', 
		CONCAT(DATEFROMPARTS(DATEPART(year, AlarmDate), DATEPART(month, AlarmDate), DATEPART(day, AlarmDate)), 'T08:00:00.0000000Z'),
		NULL
	FROM @AdvicesToAddToHF
	WHERE Scheduling = 5 AND AlarmDate > SYSUTCDATETIME()
	/* Luckily we don't have any advice with the other more complicated schedulings */

	/* Add timezone */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'TimeZoneId', 
		'UTC',
		NULL
	FROM @AdvicesToAddToHF

	/* Add queue */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'Queue', 
		'default',
		NULL
	FROM @AdvicesToAddToHF

	/* Add CreatedAt */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'CreatedAt', 
		(CONCAT(CONCAT(CONCAT(CONVERT(date, SYSUTCDATETIME()), 'T'), CONVERT(time, SYSUTCDATETIME())), 'Z')),
		NULL
	FROM @AdvicesToAddToHF

	/* Add V (pretty sure it's version) */
	INSERT INTO [kitos_HangfireDB].[HangFire].[Hash]
	(
		[Key],
		[Field],
		[Value],
		[ExpireAt]
	)
	SELECT 
		CONCAT('recurring-job:Advice: ', AdviceId),
		'V', 
		2,
		NULL
	FROM @AdvicesToAddToHF

END