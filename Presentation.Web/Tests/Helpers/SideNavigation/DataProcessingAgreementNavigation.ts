﻿import NavigationHelper = require("../../Utility/NavigationHelper");

class DataProcessingRegistrationNavigation {

    private static readonly navigation = new NavigationHelper();

    public static mainPage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.mainPageSref);
    }

    public static referencePage() {
        return DataProcessingRegistrationNavigation.navigation.goToSubMenuElement(DataProcessingRegistrationNavigationSrefs.referencePageSref);
    }
}

export = DataProcessingRegistrationNavigation;

class DataProcessingRegistrationNavigationSrefs {
    static mainPageSref = "data-processing.edit-registration.main";
    static referencePageSref = "data-processing.edit-registration.reference";
}