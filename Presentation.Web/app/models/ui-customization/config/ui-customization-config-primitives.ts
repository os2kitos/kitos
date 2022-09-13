module Kitos.Models.UICustomization.Configs {

    export interface ICustomizableUINodeConfig {
        text: string;
        readOnly?: boolean;
        children?: Record<string, ICustomizableUINodeConfig>;
        fullKey?: string; // Added by the post-processor so that controllers/ui can bind to a hierarchy while getting the right key for configuration checks...
        helpText?: string;
        subtreeIsComplete?: boolean;
    }

    export interface ICustomizableUIModuleConfigBluePrint extends ICustomizableUINodeConfig {
        module: UICustomization.CustomizableKitosModule
    }

    /**
     * Post-processing for compile-time fixed ui customization configs.
     * Will walk the config tree and create full configuration keys used to enable server-side persistence as well as rich model validation client side.
     *
     * When this function is done, the TS clients can fetch the fullKey (used for the state check) by e.g. : Kitos.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint.children.interfaces.fullKey
     * @param currentLevelKey The string identifier for the "config" node
     * @param config Current level "config" node
     * @param ancestorKeys key components of the current level's ancestry
     */
    export function processConfigurationTree(currentLevelKey: string, config: ICustomizableUINodeConfig, ancestorKeys: Array<string>) {
        //Set the current level key as "{fullKeyOfParent}.currentLevelKey"
        const keyPath = [...ancestorKeys, currentLevelKey];
        config.fullKey = keyPath.join(".");

        // Process child nodes recursively to build the fullKeys at each level
        if (config.children != undefined) {
            Object.keys(config.children).forEach(key => {
                processConfigurationTree(key, config.children[key], keyPath);
            });
        }
    }

    export const helpTexts = {
        cannotChangeTab: "Det er ikke muligt at slå dette faneblad fra",
        cannotChangeTabOnlyThroughModuleConfig: "Det er kun muligt at fjerne dette faneblad ved at slå det relaterede modul fra",
        generalUiCustomizationHelpText: "Bemærk: Skjules faneblad/felt fjernes relaterede felt(er) også fra overbliksbillederne.",
        subtreeIsCompleteHelpText: "Fanenbladdet slås fra hvis alle felter/grupper fjernes, da siden ellers vil være uden indhold."
    }
}