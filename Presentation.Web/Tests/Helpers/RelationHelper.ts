import systemUsageHelper = require("./SystemUsageHelper");
import relationPage = require("../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import Select2 = require("./Select2Helper");
import Select = require("./SelectHelper");

class RelationHelper {
    private static readonly exhibitSystemSelectId = "s2id_RelationSystemExposed";
    private static readonly relationInterfaceSelectId = "relationInterfacesSelect";
    private static readonly relationFrequencyTypeSelectId = "relationPaymentFrequenciesSelect";
    private static readonly relationContractSelectId = "relationContractsSelect";

    public static createRelation(
        fromSystemName: string,
        toSystemName: string,
        interfaceName: string,
        frequencyType: string,
        contractName: string,
        referenceText: string,
        descriptionText: string) {

        console.log("Creating relation");
        return systemUsageHelper.openLocalSystem(fromSystemName)
            .then(() => localSystemNavigation.relationsPage())
            .then(() => relationPage.getCreateButton().click())
            .then(() => Select2.searchFor(toSystemName, this.exhibitSystemSelectId))
            .then(() => Select2.waitForDataAndSelect())
            .then(() => Select.openAndSelect(this.relationInterfaceSelectId, interfaceName))
            .then(() => Select.openAndSelect(this.relationFrequencyTypeSelectId, frequencyType))
            .then(() => Select.openAndSelect(this.relationContractSelectId, contractName))
            .then(() => relationPage.getReferenceInputField().sendKeys(referenceText))
            .then(() => relationPage.getDescriptionInputField().sendKeys(descriptionText))
            .then(() => relationPage.getSaveButton().click());
    }

}
export = RelationHelper;