import OrgPage = require("../PageObjects/Global-admin/GlobalOrg.po");
import HomePage = require("../PageObjects/HomePage/HomePage.po");
import NavigationBarHelper = require("../Helpers/NavigationBarHelper");
import SystemCatalogHelper = require("./SystemCatalogHelper");

class OrgHelper {

    private static orgPage = new OrgPage();
    private static homePage = new HomePage();
    private static navigationHelper = new NavigationBarHelper();

    public static createOrg(name: string) {
        return this.orgPage.getPage()
            .then(() => this.orgPage.getCreateOrgButton().click())
            .then(() => this.orgPage.getModalOrgNameInput().sendKeys(name))
            .then(() => this.orgPage.getModalOrgRadioButton().click())
            .then(() => this.orgPage.getModalSaveNewOrgButton().click());
    }


    public static activateSystemForOrg(system: string, org: string) {
        console.log("Activating " + system + " for org " + org);
        return this.navigationHelper.dropDownExpand()
            .then(() => console.log("Dropdown clicked"))
            .then(() => browser.waitForAngular())
            .then(() => console.log("clicking change org button"))
            .then(() => this.navigationHelper.changeOrg())
            .then(() => this.homePage.selectSpecificOrganizationAsWorkingOrg(org))
            .then(() => this.homePage.selectWorkingOrganizationButton.click())
            .then(() => browser.waitForAngular())
            .then(() => SystemCatalogHelper.createLocalSystem(system));
    }

}

export = OrgHelper;