/*
Content:
    We are added Uuid to option types to be used for external api.
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

/*
Content:
    We are added Uuid to option types to be used for external api.
    As they are not generated automatically when adding the column on existing values we use this to initialize their value.
*/

BEGIN
	if (SELECT Count(*) FROM [ArchiveLocations]) >= 0
	BEGIN
		UPDATE [ArchiveLocations]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [AgreementElementTypes]) >= 0
	BEGIN
		UPDATE [AgreementElementTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [GoalTypes]) >= 0
	BEGIN
		UPDATE [GoalTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [OrganizationUnitRoles]) >= 0
	BEGIN
		UPDATE [OrganizationUnitRoles]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ReportCategoryTypes]) >= 0
	BEGIN
		UPDATE [ReportCategoryTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ItProjectTypes]) >= 0
	BEGIN
		UPDATE [ItProjectTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ItProjectRoles]) >= 0
	BEGIN
		UPDATE [ItProjectRoles]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [DataTypes]) >= 0
	BEGIN
		UPDATE [DataTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [InterfaceTypes]) >= 0
	BEGIN
		UPDATE [InterfaceTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [RelationFrequencyTypes]) >= 0
	BEGIN
		UPDATE [RelationFrequencyTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ItContractTemplateTypes]) >= 0
	BEGIN
		UPDATE [ItContractTemplateTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ItContractTypes]) >= 0
	BEGIN
		UPDATE [ItContractTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [HandoverTrialTypes]) >= 0
	BEGIN
		UPDATE [HandoverTrialTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [OptionExtendTypes]) >= 0
	BEGIN
		UPDATE [OptionExtendTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [PaymentFreqencyTypes]) >= 0
	BEGIN
		UPDATE [PaymentFreqencyTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [PaymentModelTypes]) >= 0
	BEGIN
		UPDATE [PaymentModelTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [PriceRegulationTypes]) >= 0
	BEGIN
		UPDATE [PriceRegulationTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [ProcurementStrategyTypes]) >= 0
	BEGIN
		UPDATE [ProcurementStrategyTypes]
		SET Uuid = NEWID()
	END

    if (SELECT Count(*) FROM [PurchaseFormTypes]) >= 0
	BEGIN
		UPDATE [PurchaseFormTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [ItContractRoles]) >= 0
	BEGIN
		UPDATE [ItContractRoles]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [TerminationDeadlineTypes]) >= 0
	BEGIN
		UPDATE [TerminationDeadlineTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [DataProcessingBasisForTransferOptions]) >= 0
	BEGIN
		UPDATE [DataProcessingBasisForTransferOptions]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [DataProcessingDataResponsibleOptions]) >= 0
	BEGIN
		UPDATE [DataProcessingDataResponsibleOptions]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [DataProcessingCountryOptions]) >= 0
	BEGIN
		UPDATE [DataProcessingCountryOptions]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [DataProcessingOversightOptions]) >= 0
	BEGIN
		UPDATE [DataProcessingOversightOptions]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [DataProcessingRegistrationRoles]) >= 0
	BEGIN
		UPDATE [DataProcessingRegistrationRoles]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [ItSystemRoles]) >= 0
	BEGIN
		UPDATE [ItSystemRoles]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [ArchiveTestLocations]) >= 0
	BEGIN
		UPDATE [ArchiveTestLocations]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [ArchiveTypes]) >= 0
	BEGIN
		UPDATE [ArchiveTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [ItSystemCategories]) >= 0
	BEGIN
		UPDATE [ItSystemCategories]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [SensitiveDataTypes]) >= 0
	BEGIN
		UPDATE [SensitiveDataTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [TerminationDeadlineTypesInSystems]) >= 0
	BEGIN
		UPDATE [TerminationDeadlineTypesInSystems]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [BusinessTypes]) >= 0
	BEGIN
		UPDATE [BusinessTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [RegisterTypes]) >= 0
	BEGIN
		UPDATE [RegisterTypes]
		SET Uuid = NEWID()
	END

	if (SELECT Count(*) FROM [SensitivePersonalDataTypes]) >= 0
	BEGIN
		UPDATE [SensitivePersonalDataTypes]
		SET Uuid = NEWID()
	END
END