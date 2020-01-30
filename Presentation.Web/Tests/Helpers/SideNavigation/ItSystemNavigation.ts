class ItSystemNavigation {

    public static mainPage() {
        element(by.css(`[data-ui-sref="${ItSystemNavigationSrefs.mainPageSref}"`)).click();
    }

    public static exposedInterfacesPage() {
        element(by.css(`[data-ui-sref="${ItSystemNavigationSrefs.exposedInterfacesSref}"`)).click();
    }


}

export = ItSystemNavigation;

class ItSystemNavigationSrefs {
    static mainPageSref = "it-system.edit.main";
    static exposedInterfacesSref = "it-system.edit.interfaces";
}