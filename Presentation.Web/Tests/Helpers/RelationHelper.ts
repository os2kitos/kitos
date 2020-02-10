import systemUsageHelper = require("./SystemUsageHelper");
import relationPage = require("../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import Select2 = require("./Select2Helper");

class RelationHelper {

    public static createRelation(
        fromSystemName: string,
        toSystemName: string,
        referenceText: string,
        descriptionText: string) {

        console.log("Creating relation");
        systemUsageHelper.openLocalSystem(fromSystemName)
            .then(() => localSystemNavigation.relationsPage())
            .then(() => relationPage.getCreateButton().click())
            .then(() => Select2.searchFor(toSystemName, "s2id_RelationSystemExposed"))
            .then(() => Select2.waitForDataAndSelect())
            .then(() => relationPage.getReferenceInputField().sendKeys(referenceText))
            .then(() => relationPage.getDescriptionInputField().sendKeys(descriptionText))
            .then(() => relationPage.getSaveButton().click());

    }


}
export = RelationHelper;