import NavigationHelper = require("../../Utility/NavigationHelper");

class ItSystemNavigation {

    private static readonly navigation = new NavigationHelper();

    public static mainPage() {
        return ItSystemNavigation.navigation.goToSubMenuElement(ItSystemNavigationSrefs.mainPageSref);
    }

    public static exposedInterfacesPage() {
        return ItSystemNavigation.navigation.goToSubMenuElement(ItSystemNavigationSrefs.exposedInterfacesSref);
    }
}

export = ItSystemNavigation;

class ItSystemNavigationSrefs {
    static mainPageSref = "it-system.edit.main";
    static exposedInterfacesSref = "it-system.edit.interfaces";
}