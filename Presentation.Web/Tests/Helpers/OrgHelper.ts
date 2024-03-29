﻿import OrgPage = require("../PageObjects/Global-admin/GlobalOrg.po");
import HomePage = require("../PageObjects/HomePage/HomePage.po");
import NavigationBarHelper = require("../Helpers/NavigationBarHelper");
import SystemCatalogHelper = require("./SystemCatalogHelper");

class OrgHelper {

    private static orgPage = new OrgPage();
    private static homePage = new HomePage();
    private static navigationHelper = new NavigationBarHelper();

    static createOrg(name: string) {
        return this.orgPage.getPage()
            .then(() => this.orgPage.getCreateOrgButton().click())
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalOrgNameInput().sendKeys(name))
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalOrgRadioButton().click())
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalSaveNewOrgButton().click())
            .then(() => browser.waitForAngular());
    }

    static createOrgWithCvr(name: string, cvr: string) {
        return this.orgPage.getPage()
            .then(() => this.orgPage.getCreateOrgButton().click())
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalOrgNameInput().sendKeys(name))
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalOrgCvrInput().sendKeys(cvr))
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalOrgRadioButton().click())
            .then(() => browser.waitForAngular())
            .then(() => this.orgPage.getModalSaveNewOrgButton().click())
            .then(() => browser.waitForAngular());
    }
    
    static activateSystemForOrg(system: string, org: string) {
        console.log("Activating " + system + " for org " + org);
        return this.navigationHelper.dropDownExpand()
            .then(() => console.log("Dropdown clicked"))
            .then(() => browser.waitForAngular())
            .then(() => console.log("clicking change org button"))
            .then(() => this.navigationHelper.changeOrg())
            .then(() => this.homePage.selectSpecificOrganizationAsWorkingOrg(org))
            .then(() => console.log("Organization selected"))
            .then(() => this.homePage.selectWorkingOrganizationButton.click())
            .then(() => console.log("Organization changed"))
            .then(() => browser.waitForAngular())
            .then(() => console.log("Starting system creation"))
            .then(() => SystemCatalogHelper.createLocalSystem(system));
    }

    static changeOrg(org: string) {
        console.log(`Changing org ${org}`);
        return this.navigationHelper.dropDownExpand()
            .then(() => browser.waitForAngular())
            .then(() => this.navigationHelper.changeOrg())
            .then(() => this.homePage.selectSpecificOrganizationAsWorkingOrg(org))
            .then(() => this.homePage.selectWorkingOrganizationButton.click())
            .then(() => browser.waitForAngular());
    }

}

export = OrgHelper;