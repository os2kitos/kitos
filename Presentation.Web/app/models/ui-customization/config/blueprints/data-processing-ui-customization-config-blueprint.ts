module Kitos.Models.UICustomization.Configs.BluePrints {
    export const DataProcessingUiCustomizationBluePrint = {
        module: UICustomization.CustomizableKitosModule.DataProcessingRegistrations,
        readOnly: false,
        helpText: Configs.helpTexts.generalUiCustomizationHelpText,
        text: "Databehandling",
        children: {
            frontPage: {
                text: "Forside",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTab
            },
            itSystems: {
                text: "IT Systemer",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true
            },
            itContracts: {
                text: "IT Kontrakter",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true,
                children: {
                    mainContract: {
                        text: "Hvilken kontrakt skal angive om databehandlingen er aktiv"
                    }
                }
            },
            oversight: {
                text: "Tilsyn",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true,
                children: {
                    plannedInspectionDate: {
                        text: "Kommende planlagt tilsyn"
                    }
                }
            },
            Roles: {
                text: "Databehandlingsroller"
            },
            notifications: {
                text: "Advis"
            },
            references: {
                text: "Referencer"
            }
        }
    }

    // Mandatory post-processing to build the keys
    processConfigurationTree(DataProcessingUiCustomizationBluePrint.module, DataProcessingUiCustomizationBluePrint, []);
}