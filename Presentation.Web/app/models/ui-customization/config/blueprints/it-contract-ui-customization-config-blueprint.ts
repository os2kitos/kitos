module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItContractUiCustomizationBluePrint = {
        module: UICustomization.CustomizableKitosModule.ItContract,
        readOnly: false,
        helpText: Configs.helpTexts.generalUiCustomizationHelpText,
        text: "IT Kontrakt",
        children: {
            frontPage: {
                text: "Kontraktforside",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTab
            },
            itSystems: {
                text: "IT Systemer",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true
            },
            dataProcessing: {
                text: "Databehandling",
                helpText: Configs.helpTexts.cannotChangeTabOnlyThroughModuleConfig,
                readOnly: true
            },
            deadlines: {
                text: "Aftalefrister",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            },
            paymentModel: {
                text: "Betalingsmodel",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            },
            economy: {
                text: "Økonomi",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            },
            contractRoles: {
                text: "Kontraktroller",
            },
            hierarchy: {
                text: "Hierarki",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            },
            advice: {
                text: "Advis"
            },
            references: {
                text: "Referencer",
                helpText: Configs.helpTexts.cannotChangeTab,
                readOnly: true
            }
        }
    }

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItContractUiCustomizationBluePrint.module, ItContractUiCustomizationBluePrint, []);
}