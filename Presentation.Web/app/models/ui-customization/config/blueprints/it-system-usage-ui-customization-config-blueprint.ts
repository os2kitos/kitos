module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItSystemUsageUiCustomizationBluePrint: ICustomizableUIModuleConfigBluePrint = {
        module: UICustomization.CustomizableKitosModule.ItSystemUsage,
        readOnly: false,
        children: {
            frontPage: {},
            interfaces: {},
            systemRelations: {},
            contracts: {
                children: {
                    selectContractToDetermineIfItSystemIsActive: {}
                }
            },
            hierarchy: {},
            systemRoles: {},
            organization: {},
            localKle: {},
            projects: {},
            advice: {},
            localReferences: {},
            archiving: {},
            gdpr: {}
        }
    }

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItSystemUsageUiCustomizationBluePrint.module, ItSystemUsageUiCustomizationBluePrint, []);
}