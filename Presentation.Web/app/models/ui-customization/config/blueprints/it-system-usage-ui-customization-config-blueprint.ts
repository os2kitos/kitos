module Kitos.Models.UICustomization.Configs.BluePrints {
    export const ItSystemUsageUiCustomizationBluePrint = {
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
    };

    //NOTE: Ensures that the blueprint passes the type check while still alowwing deep dive by clients into the original object
    const bluePrintTypeCheck: ICustomizableUIModuleConfigBluePrint = ItSystemUsageUiCustomizationBluePrint;

    // Mandatory post-processing to build the keys
    processConfigurationTree(ItSystemUsageUiCustomizationBluePrint.module, ItSystemUsageUiCustomizationBluePrint, []);
}