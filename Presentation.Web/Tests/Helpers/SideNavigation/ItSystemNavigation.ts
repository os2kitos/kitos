class ItSystemNavigation {

    private static  getSubMenuElement(srefName: string) {
        return element(by.css(`[data-ui-sref="${srefName}"`));
    }

    public static mainPage() {
        return ItSystemNavigation.getSubMenuElement(ItSystemNavigationSrefs.mainPageSref);
    }

    public static exposedInterfacesPage() {
        return ItSystemNavigation.getSubMenuElement(ItSystemNavigationSrefs.exposedInterfacesSref);
    }
}

export = ItSystemNavigation;

class ItSystemNavigationSrefs {
    static mainPageSref = "it-system.edit.main";
    static exposedInterfacesSref = "it-system.edit.interfaces";
}