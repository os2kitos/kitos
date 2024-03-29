﻿import ItSystemOverview = require("../PageObjects/it-system/Usage/ItSystemUsageOverview.po");
import Select2Helper = require("./Select2Helper");
import Constants = require("../Utility/Constants");

class SystemUsageHelper {
    private static pageObject = new ItSystemOverview();
    private static consts = new Constants();

    static openLocalSystem(name: string) {
        console.log(`open details for local system: ${name}`);
        return SystemUsageHelper.pageObject.getPage()
            .then(() => SystemUsageHelper.waitForKendoGrid())
            .then(() => SystemUsageHelper.findCatalogColumnsFor(name).first().click())
            .then(() => browser.waitForAngular());
    }

    static findCatalogColumnsFor(name: string) {
        return SystemUsageHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(SystemUsageHelper.pageObject.kendoToolbarWrapper.columnObjects().systemName, name);
    }

    static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return this.pageObject.waitForKendoGrid();
    }

    static selectOption(selection: string, elementId: string) {
        console.log(`Selecting value: '${selection}'`);
        return Select2Helper.selectWithNoSearch(selection, elementId);
    };

    static validateSelectData(expectedValue: string, elementId: string) {
        console.log(`Validating if select contains value: '${expectedValue}'`);
        return Select2Helper.getData(elementId).getText().then(result => expect(result.trim()).toBe(expectedValue.trim()));
    };
}
export = SystemUsageHelper;