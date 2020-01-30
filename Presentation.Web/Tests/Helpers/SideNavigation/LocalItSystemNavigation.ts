class LocalItSystemNavigation {

    public static mainPage() {
        element(by.css(`[data-ui-sref="${LocalItSystemNavigationSrefs.mainPageSref}"`)).click();
    }

    public static exposedInterfacesPage() {
        element(by.css(`[data-ui-sref="${LocalItSystemNavigationSrefs.exposedInterfacesSref}"`)).click();
    }
    
    
}

export = LocalItSystemNavigation;

class LocalItSystemNavigationSrefs {
    static mainPageSref = "it-system.usage.main";
    static exposedInterfacesSref = "it-system.usage.interfaces";
}