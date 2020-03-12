import NavigationHelper = require("../../Utility/NavigationHelper");

class LocalItSystemNavigation {

    private static readonly navigation = new NavigationHelper();

    public static mainPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.mainPageSref);
    }

    public static exposedInterfacesPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.exposedInterfacesSref);
    }

    public static relationsPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.relationsSref);
    }
    
    public static openArchivingPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.archivingSref);
    }
}

export = LocalItSystemNavigation;

class LocalItSystemNavigationSrefs {
    static mainPageSref = "it-system.usage.main";
    static exposedInterfacesSref = "it-system.usage.interfaces";
    static relationsSref = "it-system.usage.relation";
    static archivingSref = "it-system.usage.archiving";
}