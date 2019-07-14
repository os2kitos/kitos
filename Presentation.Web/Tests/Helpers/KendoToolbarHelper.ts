import KendoToolbarWrapper = require("../object-wrappers/KendoToolbarWrapper");

var kendoToolbarWrapper = new KendoToolbarWrapper();
class KendoToolbarHelper {

    public headerButtons = new headerButtons();
   
}

class headerButtons {
    public isDeleteDisabled(): webdriver.promise.Promise<String> {
        return kendoToolbarWrapper.headerButtons().deleteFilter.getAttribute("disabled");
    }

    public isUseDisabled(): webdriver.promise.Promise<String> {
        return kendoToolbarWrapper.headerButtons().useFilter.getAttribute("disabled");
    }

}

export = KendoToolbarHelper;