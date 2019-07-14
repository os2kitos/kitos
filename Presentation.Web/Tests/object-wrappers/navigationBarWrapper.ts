import cssLocator = require("./cssLocatorHelper");

type navigations = {
    logo: protractor.ElementFinder,
    organization: protractor.ElementFinder,
    project: protractor.ElementFinder,
    system: protractor.ElementFinder,
    contract: protractor.ElementFinder,
    reports: protractor.ElementFinder
}
type userDropdown = {
    dropDownElement: protractor.ElementFinder,
    myProfile: protractor.ElementFinder,
    localAdmin: protractor.ElementFinder,
    globalAdmin: protractor.ElementFinder,
    changeOrg: protractor.ElementFinder,
    logOut: protractor.ElementFinder
}

var byDataElementType = new cssLocator().byDataElementType;

class navigationBarWrapper {

    public headerNavigations: navigations = {
        logo: element(byDataElementType("kitosLogo")),
        organization: element(byDataElementType("organizationButton")),
        project: element(byDataElementType("projectButton")),
        system: element(byDataElementType("systemButton")),
        contract: element(byDataElementType("contractButton")),
        reports: element(byDataElementType("reportButton"))
        };

    public dropDownMenu: userDropdown = {
        dropDownElement: element(by.id("dropdown-button")),
        myProfile: element(byDataElementType("myProfileAnchor")),
        localAdmin: element(byDataElementType("localAdminAnchor")),
        globalAdmin: element(byDataElementType("globalAdminAnchor")),
        changeOrg: element(byDataElementType("changeOrganizationAnchor")),
        logOut: element(byDataElementType("logOutAnchor"))
        };

}

export = navigationBarWrapper;