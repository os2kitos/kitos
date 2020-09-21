import NavigationHelper = require("../../Utility/NavigationHelper");

class DataProcessingAgreementNavigation {

    private static readonly navigation = new NavigationHelper();

    public static mainPage() {
        return DataProcessingAgreementNavigation.navigation.goToSubMenuElement(DataProcessingAgreementNavigationSrefs.mainPageSref);
    }

    public static referencePage() {
        return DataProcessingAgreementNavigation.navigation.goToSubMenuElement(DataProcessingAgreementNavigationSrefs.referencePageSref);
    }
}

export = DataProcessingAgreementNavigation;

class DataProcessingAgreementNavigationSrefs {
    static mainPageSref = "data-processing.edit-agreement.main";
    static referencePageSref = "data-processing.edit-agreement.reference";
}