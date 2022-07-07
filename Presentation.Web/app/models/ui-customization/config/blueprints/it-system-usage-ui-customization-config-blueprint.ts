﻿module Kitos.Models.UICustomization.Configs.BluePrints {
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
            interfaces: {
                text: "Udstillede snitflader"
            },
            systemRelations: {
                text: "Relationer"
            },
            dataProcessings: {
                text: "Databehandling"
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
            hierarchy: {
                text: "Hierarki"
            },
            systemRoles: {
                text: "Systemroller"
            },
            organization: {
                text: "Organisation"
            },
            localKle: {
                text: "Lokale KLE"
            },
            projects: {
                text: "Projekter",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
            },
            advice: {
                text: "Advis"
            },
            localReferences: {
                text: "Lokale referencer"
            },
            archiving: {
                text: "Arkivering"
            },
            gdpr: {
                text: "GDPR"
            }
        }
    };

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItSystemUsageUiCustomizationBluePrint.module, ItSystemUsageUiCustomizationBluePrint, []);
}