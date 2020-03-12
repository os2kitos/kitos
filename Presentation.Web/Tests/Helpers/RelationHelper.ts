import systemUsageHelper = require("./SystemUsageHelper");
import relationPage = require("../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import Select2 = require("./Select2Helper");

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
            .then(() => Select2.select(toSystemName, this.exhibitSystemSelectId))
            .then(() => Select2.selectWithNoSearch(interfaceName, this.relationInterfaceSelectId))
            .then(() => Select2.selectWithNoSearch(frequencyType, this.relationFrequencyTypeSelectId))
            .then(() => Select2.select(contractName, this.relationContractSelectId))
            .then(() => relationPage.getReferenceInputField().sendKeys(referenceText))
            .then(() => relationPage.getDescriptionInputField().sendKeys(descriptionText))
            .then(() => relationPage.getSaveButton().click());
    }

    public static editRelation(
        fromSystemName: string,
        toSystemName: string,
        interfaceName: string,
        frequencyType: string,
        contractName: string,
        referenceText: string,
        descriptionText: string) {

        console.log("Editing relation");
        return systemUsageHelper.openLocalSystem(fromSystemName)
            .then(() => localSystemNavigation.relationsPage())
            .then(() => relationPage.getEditButton(toSystemName).click())
            .then(() => Select2.selectWithNoSearch(interfaceName, this.relationInterfaceSelectId))
            .then(() => Select2.selectWithNoSearch(frequencyType, this.relationFrequencyTypeSelectId))
            .then(() => Select2.select(contractName, this.relationContractSelectId))
            .then(() => relationPage.getReferenceInputField().clear())
            .then(() => relationPage.getReferenceInputField().sendKeys(referenceText))
            .then(() => relationPage.getDescriptionInputField().clear())
            .then(() => relationPage.getDescriptionInputField().sendKeys(descriptionText))
            .then(() => relationPage.getSaveButton().click())
            .then(() => browser.waitForAngular());
    }

    public static deleteRelation(fromSystemName: string, toSystemName: string) {
        console.log("Deleting relation " + toSystemName);
        return systemUsageHelper.openLocalSystem(fromSystemName)
            .then(() => localSystemNavigation.relationsPage())
            .then(() => relationPage.getEditButton(toSystemName).click())
            .then(() => relationPage.getDeleteButton().click())
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular())
            .then(() => browser.refresh())
            // Refreshing because of a bug when running in debug mode 
            // which causes modal event listener to fire twice
            .then(() => browser.waitForAngular());
    }

}
export = RelationHelper;
