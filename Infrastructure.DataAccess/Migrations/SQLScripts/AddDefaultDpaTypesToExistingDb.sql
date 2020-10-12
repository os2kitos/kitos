/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1163
 
Content:
    In order to facilitate automatic migration of data from IT-System and IT-Contract, add default values for DPA Options.
    NOTE: Only modifies existing db. Test db is created in a test db task
*/

BEGIN
    if((SELECT [Id] FROM [dbo].[User] WHERE [dbo].[User].[Email] = 'support@kitos.dk') IS NOT NULL)
    BEGIN
		DECLARE @defaultUserId AS INT
		SELECT @defaultUserId = (SELECT [Id] FROM [dbo].[User] WHERE [dbo].[User].[Email] = 'support@kitos.dk');

        INSERT INTO [dbo].[DataProcessingOversightOptions]
           ([Name]
           ,[IsLocallyAvailable]
           ,[IsObligatory]
           ,[IsSuggestion]
           ,[Description]
           ,[IsEnabled]
           ,[Priority]
           ,[ObjectOwnerId]
           ,[LastChanged]
           ,[LastChangedByUserId])
        VALUES
        ('Egen kontrol',            1,  1,  0,  null,   1,  0,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Ledelseserklæring',       1,  1,  0,  null,   1,  1,  @defaultUserId, GetDate(),  @defaultUserId),
        ('ISAE 3000',               1,  1,  0,  null,   1,  2,  @defaultUserId, GetDate(),  @defaultUserId),
        ('ISAE 3402 type 1',        1,  1,  0,  null,   1,  3,  @defaultUserId, GetDate(),  @defaultUserId),
        ('ISAE 3402 type 2',        1,  1,  0,  null,   1,  4,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Skriftlig kontrol',       1,  1,  0,  null,   1,  5,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Fysisk tilsyn',           1,  1,  0,  null,   1,  6,  @defaultUserId, GetDate(),  @defaultUserId)

        INSERT INTO [dbo].[DataProcessingBasisForTransferOptions]
           ([Name]
           ,[IsLocallyAvailable]
           ,[IsObligatory]
           ,[IsSuggestion]
           ,[Description]
           ,[IsEnabled]
           ,[Priority]
           ,[ObjectOwnerId]
           ,[LastChanged]
           ,[LastChangedByUserId])
        VALUES
        ('EU og EØS',                                                   1,  1,  0,  null,   1,  0,  @defaultUserId, GetDate(),  @defaultUserId),
        ('EU''s standard kontrakt /standard contract clauses',          1,  1,  0,  null,   1,  1,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Binding corporate rules / BCR',                               1,  1,  0,  null,   1,  2,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Intet',                                                       1,  1,  0,  null,   1,  3,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Andet',                                                       1,  1,  0,  null,   1,  4,  @defaultUserId, GetDate(),  @defaultUserId)

        INSERT INTO [dbo].[DataProcessingDataResponsibleOptions]
           ([Name]
           ,[IsLocallyAvailable]
           ,[IsObligatory]
           ,[IsSuggestion]
           ,[Description]
           ,[IsEnabled]
           ,[Priority]
           ,[ObjectOwnerId]
           ,[LastChanged]
           ,[LastChangedByUserId])
        VALUES
        ('Leverandøren er databehandler',                   1,  1,  0,  '(Det er vurderet at leverandøren behandler persondata på instruks fra kommunen og der skal indgås en databehandleraftale)',   1,  0,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Leverandøren behandler ikke personoplysninger',   1,  1,  0,  '(Og derfor skal der ikke indgås en databehandleraftale)',   1,  1,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Leverandøren er selvstændig dataansvarlig',       1,  1,  0,  '(Deres anvendelse af data er ikke noget vi har indflydelse på)',   1,  2,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Fællesdataansvar',                                1,  1,  0,  '(Der skal typisk indgås en anden type aftale – fortrolighedserklæring eller…)',   1,  3,  @defaultUserId, GetDate(),  @defaultUserId),
        ('Kommunen er selv dataansvarlig',                  1,  1,  0,  null,   1,  4,  @defaultUserId, GetDate(),  @defaultUserId)
	END
END