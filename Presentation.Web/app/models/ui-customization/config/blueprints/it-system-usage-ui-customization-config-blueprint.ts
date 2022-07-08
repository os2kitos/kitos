module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItSystemUsageUiCustomizationBluePrint = {
        module: UICustomization.CustomizableKitosModule.ItSystemUsage,
        readOnly: false,
        helpText: Configs.helpTexts.generalUiCustomizationHelpText,
        text: "IT-Systemer i anvendelse",
        children: {
            frontPage: {
                text: "Systemforside",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTab
            },
            contracts: {
                text: "Kontrakter",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                children: {
                    selectContractToDetermineIfItSystemIsActive: {
                        text: "Hvilken kontrakt skal afgøre om IT systemet er aktivt"
                    }
                }
            },
            projects: {
                text: "Projekter",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
            },
            dataProcessing: {
                text: "Databehandling",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
            },
            gdpr: {
                text: "GDPR"
            },
            systemRoles: {
                text: "Systemroller"
            },
            organization: {
                text: "Organisation"
            },
            systemRelations: {
                text: "Relationer"
            },
            interfaces: {
                text: "Udstillede snitflader"
            },
            archiving: {
                text: "Arkivering"
            },
            hierarchy: {
                text: "Hierarki"
            },
            localKle: {
                text: "Lokale KLE"
            },
            advice: {
                text: "Advis"
            },
            localReferences: {
                text: "Lokale referencer"
            }
        }
    };

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItSystemUsageUiCustomizationBluePrint.module, ItSystemUsageUiCustomizationBluePrint, []);
}