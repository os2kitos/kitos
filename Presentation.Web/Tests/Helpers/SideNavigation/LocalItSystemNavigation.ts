import NavigationHelper = require("../../Utility/NavigationHelper");
import LocalItSystemNavigationSrefs = require("./LocalItSystemNavigationSrefs");

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

    public static openGDPRPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.GDPRSref);
    }

    public static openDataProcessingPage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.dataProcessingSref);
    }

    public static openAdvicePage() {
        return LocalItSystemNavigation.navigation.goToSubMenuElement(LocalItSystemNavigationSrefs.adviceSref);
    }
}

export = LocalItSystemNavigation;