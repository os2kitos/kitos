import cssLocator = require("./cssLocatorHelper");

type navigations = {
    logo: protractor.ElementFinder, organization: protractor.ElementFinder, project: protractor.ElementFinder, system: protractor.ElementFinder,
    contract: protractor.ElementFinder, reports: protractor.ElementFinder
}
type userDropdown = {
    dropDownElement: protractor.ElementFinder, myProfile: protractor.ElementFinder, localAdmin: protractor.ElementFinder, globalAdming: protractor.ElementFinder,
    changeOrg: protractor.ElementFinder, logOut: protractor.ElementFinder
}



var byHook = new cssLocator().byDataHook;

class navigationBarWrapper {
    

    public headerNavigations(): navigations {
        var nav: navigations = {
            logo: element(byHook("kitosLogo")),
            organization: element(byHook("organizationButton")),
            project: element(byHook("projectButton")),
            system: element(byHook("systemButton")),
            contract: element(byHook("contractButton")),
            reports: element(byHook("reportButton"))
        };
        return nav;
    }

    public dropDown(): userDropdown {
        var drop: userDropdown = {
            dropDownElement: element(by.id("dropdown-button")),
            myProfile: element(byHook("myProfile")),
            localAdmin: element(byHook("localAdmin")),
            globalAdming: element(byHook("globalAdmin")),
            changeOrg: element(byHook("changeOrganization")),
            logOut: element(byHook("logout"))
        };
        return drop;
    }

}

export = navigationBarWrapper;