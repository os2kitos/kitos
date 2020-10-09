import NavigationHelper = require("../../Utility/NavigationHelper");

class DataProcessingRegistrationNavigation {

    private static readonly navigation = new NavigationHelper();

    public static mainPage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.mainPageSref);
    }

    public static referencePage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.referencePageSref);
    }

    public static oversightPage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.oversightPageSref);
    }

    public static contractPage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.contractPageSref);
    }
}

export = DataProcessingRegistrationNavigation;

class DataProcessingRegistrationNavigationSrefs {
    static mainPageSref = "data-processing.edit-registration.main";
    static referencePageSref = "data-processing.edit-registration.reference";
    static oversightPageSref = "data-processing.edit-registration.oversight";
    static contractPageSref = "data-processing.edit-registration.contracts";
}