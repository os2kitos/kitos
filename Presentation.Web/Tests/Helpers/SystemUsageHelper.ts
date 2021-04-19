import ItSystemOverview = require("../PageObjects/it-system/Usage/ItSystemUsageOverview.po");

class SystemUsageHelper {
    private static pageObject = new ItSystemOverview();

    static openLocalSystem(name: string) {
        console.log(`open details for local system: ${name}`);
        return SystemUsageHelper.pageObject.getPage()
            .then(() => SystemUsageHelper.waitForKendoGrid())
            .then(() => SystemUsageHelper.findCatalogColumnsFor(name).first().click());
    }

    static findCatalogColumnsFor(name: string) {
        return SystemUsageHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(SystemUsageHelper.pageObject.kendoToolbarWrapper.columnObjects().systemName, name);
    }

    static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return this.pageObject.waitForKendoGrid();
    }
}
export = SystemUsageHelper;