import KendoToolbarWrapper = require("../object-wrappers/KendoToolbarWrapper");

class KendoToolbarHelper {
    public headerButtons = new HeaderButtons();
}

class HeaderButtons {
    private kendoToolbarWrapper = new KendoToolbarWrapper();

    public isDeleteDisabled(): webdriver.promise.Promise<String> {
        return this.kendoToolbarWrapper.headerButtons().deleteFilter.getAttribute("disabled");
    }

    public isUseDisabled(): webdriver.promise.Promise<String> {
        return this.kendoToolbarWrapper.headerButtons().useFilter.getAttribute("disabled");
    }

}

export = KendoToolbarHelper;