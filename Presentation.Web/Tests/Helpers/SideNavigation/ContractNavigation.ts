class ContractNavigation {

    public static openMainPage() {
        element(by.css(`[data-ui-sref="${ContractNavigationSrefs.mainPageSref}"`)).click();
    }

    public static openSystemsPage() {
        element(by.css(`[data-ui-sref="${ContractNavigationSrefs.systemPageSref}"`)).click();
    }
}

export = ContractNavigation;

class ContractNavigationSrefs {
    static mainPageSref = "it-contract.edit.main";
    static systemPageSref = "it-contract.edit.systems";
}