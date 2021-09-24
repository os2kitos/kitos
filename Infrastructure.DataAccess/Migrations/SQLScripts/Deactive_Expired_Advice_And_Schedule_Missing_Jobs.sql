/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-2243
 
Content:
	- Removes old Hangfire jobs which have no related data and should be deleted.
	- Updates IsActive column on Advice, which should be deactivated when Hangfire runs next send. But won't be as there is no Hangfire job
	- Recreates Hangfire jobs on Advices where there should be a job but isn't one.
*/

/* Note: Check to see if Hangfire tables exists so that script doesn't run on fresh install */
IF OBJECT_ID('kitos_HangfireDB.HangFire.Set', 'U') IS NOT NULL AND 
	OBJECT_ID('kitos_HangfireDB.HangFire.Hash', 'U') IS NOT NULL AND 
	OBJECT_ID('kitos_HangfireDB.HangFire.Job', 'U') IS NOT NULL AND 
	OBJECT_ID('kitos_HangfireDB.HangFire.JobParameter', 'U') IS NOT NULL

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
		WHERE Scheduling IS NOT NULL AND StopDate IS NOT NULL AND Scheduling != 0 AND StopDate < GETUTCDATE() AND IsActive = 1
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
			AlarmDate datetime2
		)
	
		INSERT INTO @AdvicesToAddToHF
		SELECT [Id], [AlarmDate]
		FROM [Kitos].[dbo].[Advice]
		WHERE Scheduling IS NOT NULL AND Scheduling != 0 AND (StopDate >= GETUTCDATE() OR StopDate IS NULL) AND IsActive = 1
		AND [Id] NOT IN (
			SELECT *
			FROM @HFAdvice
		)

	
		/* Part 5: Add to hangfire as scheduled jobs */
		DECLARE @NewHFJobs TABLE
		(
			[JobId] int,
			[AdviceId] int
		)


		INSERT INTO [kitos_HangfireDB].[HangFire].[Job] ([StateName], [InvocationData], [Arguments], [CreatedAt], [ExpireAt])
		OUTPUT INSERTED.Id, REVERSE(SUBSTRING(REVERSE(SUBSTRING(INSERTED.Arguments, 3, 8)), 3, 8)) INTO @NewHFJobs ([JobId], [AdviceId])
		SELECT
			'Scheduled',
			'{"Type":"Core.ApplicationServices.AdviceService, Core.ApplicationServices, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null","Method":"CreateOrUpdateJob","ParameterTypes":"[\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"]","Arguments":null}',
			CONCAT('["', Advices.[AdviceId], '"]'),
			SYSUTCDATETIME(),
			DATEADD(month, 1, SYSUTCDATETIME())
		FROM @AdvicesToAddToHF AS Advices

		INSERT INTO [kitos_HangfireDB].[HangFire].[JobParameter] ([JobId], [Name], [Value])
		SELECT 
			[JobId],
			'CurrentCulture',
			'"en-DK"'
		FROM @NewHFJobs

		INSERT INTO [kitos_HangfireDB].[HangFire].[JobParameter] ([JobId], [Name], [Value])
		SELECT 
			[JobId],
			'CurrentUICulture',
			'"en-US"'
		FROM @NewHFJobs

		INSERT INTO [kitos_HangfireDB].[HangFire].[State] ([JobId], [Name], [Reason], [CreatedAt], [Data])
		SELECT 
			NewJobs.[JobId],
			'Scheduled', 
			NULL,
			SYSUTCDATETIME(), 
			CONCAT('{"EnqueueAt":"', CONVERT(date, Advices.AlarmDate), 'T', CONVERT(time, Advices.AlarmDate), 'Z","ScheduledAt":"', CONVERT(date, SYSUTCDATETIME()), 'T', CONVERT(time, SYSUTCDATETIME()), 'Z', '"}')
		FROM @NewHFJobs AS NewJobs
			INNER JOIN
			@AdvicesToAddToHF AS Advices ON NewJobs.AdviceId = Advices.AdviceId

		UPDATE [kitos_HangfireDB].[HangFire].[Job]
		SET [Job].[StateId] = [State].[Id]
		FROM 
			[kitos_HangfireDB].[HangFire].[State] AS [State]
			INNER JOIN 
			[kitos_HangfireDB].[HangFire].[Job] AS [Job]
			ON [Job].Id = [State].JobId

		UPDATE [kitos_HangfireDB].[HangFire].[Job]
		SET [ExpireAt] = NULL
		WHERE [Id] IN (
			SELECT [JobId]
			FROM @NewHFJobs
		)

		DECLARE @JobsWhichHasPassedAlarmDate TABLE
		(
			[JobId] int
		)

		INSERT INTO @JobsWhichHasPassedAlarmDate
		SELECT NewJobs.JobId
		FROM @NewHFJobs AS NewJobs
			INNER JOIN
			@AdvicesToAddToHF AS Advices ON NewJobs.AdviceId = Advices.AdviceId
		WHERE Advices.AlarmDate <= SYSUTCDATETIME()


		DECLARE @JobsHasYetToPassAlarmDate TABLE
		(
			[JobId] int,
			[AlarmDate] datetime2
		)

		INSERT INTO @JobsHasYetToPassAlarmDate ([JobId], [AlarmDate])
		SELECT NewJobs.[JobId], Advices.AlarmDate
		FROM @NewHFJobs AS NewJobs
			INNER JOIN
			@AdvicesToAddToHF AS Advices ON NewJobs.AdviceId = Advices.AdviceId
		WHERE Advices.AlarmDate > SYSUTCDATETIME()
	
		/* When adding to SET the [Score] which is EPOCH decides when to run the scheduled job. */
		MERGE [kitos_HangfireDB].[HangFire].[Set] WITH (holdlock) AS [Target]
		USING @JobsWhichHasPassedAlarmDate AS [Source]
		ON [Target].[Key] = [Source].[JobId] AND [Target].[Value] = 'schedule'
		WHEN MATCHED THEN UPDATE SET [Score] = DATEDIFF_BIG(SECOND, DATEFROMPARTS(1970, 1, 1), SYSUTCDATETIME())
		WHEN NOT MATCHED THEN INSERT ([Key], [Score], [Value]) VALUES ('schedule', DATEDIFF_BIG(SECOND, DATEFROMPARTS(1970, 1, 1), SYSUTCDATETIME()), [Source].[JobId]);


		MERGE [kitos_HangfireDB].[HangFire].[Set] WITH (holdlock) AS [Target]
		USING @JobsHasYetToPassAlarmDate AS [Source]
		ON [Target].[Key] = [Source].[JobId] AND [Target].[Value] = 'schedule'
		WHEN MATCHED THEN UPDATE SET [Score] = DATEDIFF_BIG(SECOND, DATEFROMPARTS(1970, 1, 1), [AlarmDate])
		WHEN NOT MATCHED THEN INSERT ([Key], [Score], [Value]) VALUES ('schedule', DATEDIFF_BIG(SECOND, DATEFROMPARTS(1970, 1, 1), [AlarmDate]), [Source].[JobId]);

	END
ELSE
	BEGIN
		PRINT('Could not find Hangfire DB so skips Advice Cleanup and Re-schedule SQL script')
	END