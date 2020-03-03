import NavigationHelper = require("../../Utility/NavigationHelper");

class ContractNavigation {

    private static readonly navigation = new NavigationHelper();

    public static openMainPage() {
        return ContractNavigation.navigation.goToSubMenuElement(ContractNavigationSrefs.mainPageSref);
    }

    public static openSystemsPage() {
        return ContractNavigation.navigation.goToSubMenuElement(ContractNavigationSrefs.systemPageSref);
    }
}

export = ContractNavigation;

class ContractNavigationSrefs {
    static mainPageSref = "it-contract.edit.main";
    static systemPageSref = "it-contract.edit.systems";
}