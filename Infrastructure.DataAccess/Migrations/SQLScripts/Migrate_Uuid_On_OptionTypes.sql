/*
Content:
    We are added Uuid to option types to be used for external api.
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
    UPDATE [ArchiveLocations]
    SET Uuid = NEWID()

    UPDATE [AgreementElementTypes]
    SET Uuid = NEWID()

    UPDATE [GoalTypes]
    SET Uuid = NEWID()

    UPDATE [OrganizationUnitRoles]
    SET Uuid = NEWID()

    UPDATE [ReportCategoryTypes]
    SET Uuid = NEWID()

    UPDATE [ItProjectTypes]
    SET Uuid = NEWID()

    UPDATE [ItProjectRoles]
    SET Uuid = NEWID()

    UPDATE [DataTypes]
    SET Uuid = NEWID()

    UPDATE [InterfaceTypes]
    SET Uuid = NEWID()

    UPDATE [RelationFrequencyTypes]
    SET Uuid = NEWID()

    UPDATE [ItContractTemplateTypes]
    SET Uuid = NEWID()

    UPDATE [ItContractTypes]
    SET Uuid = NEWID()

    UPDATE [HandoverTrialTypes]
    SET Uuid = NEWID()

    UPDATE [OptionExtendTypes]
    SET Uuid = NEWID()

    UPDATE [PaymentFreqencyTypes]
    SET Uuid = NEWID()

    UPDATE [PaymentModelTypes]
    SET Uuid = NEWID()

    UPDATE [PriceRegulationTypes]
    SET Uuid = NEWID()

    UPDATE [ProcurementStrategyTypes]
    SET Uuid = NEWID()

    UPDATE [PurchaseFormTypes]
    SET Uuid = NEWID()

    UPDATE [ItContractRoles]
    SET Uuid = NEWID()

    UPDATE [TerminationDeadlineTypes]
    SET Uuid = NEWID()

    UPDATE [DataProcessingBasisForTransferOptions]
    SET Uuid = NEWID()

    UPDATE [DataProcessingDataResponsibleOptions]
    SET Uuid = NEWID()

    UPDATE [DataProcessingCountryOptions]
    SET Uuid = NEWID()

    UPDATE [DataProcessingOversightOptions]
    SET Uuid = NEWID()

    UPDATE [DataProcessingRegistrationRoles]
    SET Uuid = NEWID()

    UPDATE [ItSystemRoles]
    SET Uuid = NEWID()

    UPDATE [ArchiveTestLocations]
    SET Uuid = NEWID()

    UPDATE [ArchiveTypes]
    SET Uuid = NEWID()

    UPDATE [ItSystemCategories]
    SET Uuid = NEWID()

    UPDATE [SensitiveDataTypes]
    SET Uuid = NEWID()

    UPDATE [TerminationDeadlineTypesInSystems]
    SET Uuid = NEWID()

    UPDATE [BusinessTypes]
    SET Uuid = NEWID()

    UPDATE [RegisterTypes]
    SET Uuid = NEWID()

    UPDATE [SensitivePersonalDataTypes]
    SET Uuid = NEWID()
END