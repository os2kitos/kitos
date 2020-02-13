import systemUsageHelper = require("./SystemUsageHelper");
import relationPage = require("../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import Select2 = require("./Select2Helper");
import Select = require("./SelectHelper");

class RelationHelper {
    private static readonly exhibitSystemSelectId = "s2id_RelationSystemExposed";
    private static readonly relationInterfaceSelectId = "s2id_RelationInterfacesSelect";
    private static readonly relationFrequencyTypeSelectId = "s2id_RelationPaymentFrequenciesSelect";
    private static readonly relationContractSelectId = "s2id_RelationContractsSelect";

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
            .then(() => Select2.selectWithNoSearch(interfaceName, this.relationInterfaceSelectId))
            .then(() => Select2.selectWithNoSearch(frequencyType, this.relationFrequencyTypeSelectId))
            .then(() => Select2.searchFor(contractName, this.relationContractSelectId))
            .then(() => Select2.waitForDataAndSelect())
            .then(() => relationPage.getReferenceInputField().sendKeys(referenceText))
            .then(() => relationPage.getDescriptionInputField().sendKeys(descriptionText))
            .then(() => relationPage.getSaveButton().click());
    }

}
export = RelationHelper;