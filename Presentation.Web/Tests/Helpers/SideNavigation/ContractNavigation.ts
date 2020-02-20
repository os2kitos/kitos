class ContractNavigation {

    public static mainPage() {
        element(by.css(`[data-ui-sref="${ContractNavigationSrefs.mainPageSref}"`)).click();
    }

    public static systemsPage() {
        element(by.css(`[data-ui-sref="${ContractNavigationSrefs.exposedInterfacesSref}"`)).click();
    }
}

export = ContractNavigation;

class ContractNavigationSrefs {
    static mainPageSref = "it-contract.edit.main";
    static exposedInterfacesSref = "it-contract.edit.systems";
}