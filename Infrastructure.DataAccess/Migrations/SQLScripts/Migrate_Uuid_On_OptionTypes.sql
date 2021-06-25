
/*
Content:
    We added Uuid to option types to be used for external api: https://os2web.atlassian.net/browse/KITOSUDV-1709.
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
    UPDATE [ArchiveLocations]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
    
    UPDATE [AgreementElementTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [GoalTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [OrganizationUnitRoles]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ReportCategoryTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItProjectTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItProjectRoles]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [InterfaceTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [RelationFrequencyTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItContractTemplateTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItContractTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [HandoverTrialTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [OptionExtendTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [PaymentFreqencyTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [PaymentModelTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [PriceRegulationTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ProcurementStrategyTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [PurchaseFormTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItContractRoles]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [TerminationDeadlineTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataProcessingBasisForTransferOptions]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataProcessingDataResponsibleOptions]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataProcessingCountryOptions]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataProcessingOversightOptions]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [DataProcessingRegistrationRoles]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItSystemRoles]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ArchiveTestLocations]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ArchiveTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [ItSystemCategories]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [SensitiveDataTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [TerminationDeadlineTypesInSystems]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [BusinessTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [RegisterTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL

    UPDATE [SensitivePersonalDataTypes]
    SET Uuid = NEWID()
    WHERE Uuid = '00000000-0000-0000-0000-000000000000' OR Uuid IS NULL
END