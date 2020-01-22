import CssLocator = require("./cssLocatorHelper");
import Constants = require("../Utility/Constants");

type navigations = {
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
var byDataElementType = new CssLocator().byDataElementType;
var consts = new Constants();

class navigationBarWrapper {

    public headerNavigations: navigations = {
        organization: element(byDataElementType(consts.navigationOrganizationButton)),
        project: element(byDataElementType(consts.navigationProjectButton)),
        system: element(byDataElementType(consts.navigationSystemButton)),
        contract: element(byDataElementType(consts.navigationContractButton)),
        reports: element(byDataElementType(consts.navigationReportsButton))
        };

    public dropDownMenu: userDropdown = {
        dropDownElement: element(by.id(consts.navigationDropdown)),
        myProfile: element(byDataElementType(consts.navigationDropdownMyProfile)),
        localAdmin: element(byDataElementType(consts.navigationDropdownLocalAdmin)),
        globalAdmin: element(byDataElementType(consts.navigationDropdownGlobalAdmin)),
        changeOrg: element(byDataElementType(consts.navigationDropdownChangeOrg)),
        logOut: element(byDataElementType(consts.navigationDropdownLogOut))
        };
}
export = navigationBarWrapper;