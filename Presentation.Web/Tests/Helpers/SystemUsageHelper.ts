import ItSystemOverview = require("../PageObjects/It-system/ItSystemOverview.po");
import WaitTimers = require("../Utility/WaitTimers");

class SystemUsageHelper {
    private static pageObject = new ItSystemOverview();
    private static waitUpTo = new WaitTimers();

    public static openLocalSystem(name: string) {
        console.log(`open details for local system: ${name}`);
        return SystemUsageHelper.pageObject.getPage()
            .then(() => SystemUsageHelper.waitForKendoGrid())
            .then(() => SystemUsageHelper.findCatalogColumnsFor(name).first().click());
    }

    public static findCatalogColumnsFor(name: string) {
        return SystemUsageHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(SystemUsageHelper.pageObject.kendoToolbarWrapper.columnObjects().systemName, name);
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(SystemUsageHelper.pageObject.waitForKendoGrid(), SystemUsageHelper.waitUpTo.twentySeconds);
    }
}
export = SystemUsageHelper;