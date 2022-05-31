module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItContractUiCustomizationBluePrint = {
        module: UICustomization.CustomizableKitosModule.ItContract,
        readOnly: false,
        //TODO: Replace the text
        helpText: "INSERT TEXT",
        text: "Kontraktoverblik - Økonomi",
        children: {
            frontPage: {
                text: "Kontraktforside",
                readOnly: true,
                helpText: Configs.helpTexts.cannotChangeTab
            },
            itSystems: {
                text: "IT Systemer",
                readOnly: true
            },
            dataProcessing: {
                text: "Databehandling",
                readOnly: true
            },
            deadlines: {
                text: "Aftalefrister",
                readOnly: true
            },
            paymentModel: {
                text: "Betalingsmodel",
                readOnly: true
            },
            economy: {
                text: "Økonomi",
                readOnly: true
            },
            contractRoles: {
                text: "Kontraktroller",
            },
            hierarchy: {
                text: "Hierarki",
                readOnly: true
            },
            advice: {
                text: "Advis"
            },
            references: {
                text: "Referencer",
                readOnly: true
            }
        }
    }

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItContractUiCustomizationBluePrint.module, ItContractUiCustomizationBluePrint, []);
}