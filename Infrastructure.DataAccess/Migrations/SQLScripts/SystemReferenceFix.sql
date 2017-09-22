  UPDATE [dbo].[ExternalReferences]
  SET [URL] = [ExternalReferenceId]
  WHERE ItSystem_Id IS NOT NULL AND ExternalReferenceId LIKE '%http%'

  GO

  UPDATE [dbo].[ExternalReferences]
  SET [ExternalReferenceId] = NULL
  WHERE ItSystem_Id IS NOT NULL AND ExternalReferenceId LIKE '%http%'

  GO