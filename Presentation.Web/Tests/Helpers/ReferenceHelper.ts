import ReferencePage = require("../PageObjects/It-system/Tabs/ItSystemReference.po");
import WaitTimers = require("../Utility/WaitTimers");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import ItSystemCatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");

class ReferenceHelper {
    private homePage = new ReferencePage();
    private itSystemCatalogPage = new ItSystemCatalogPage();
    private waitUpTo = new WaitTimers();
    private headerButtons = new ReferencePage().kendoToolbarWrapper.headerButtons();
    private inputFields = new ReferencePage().kendoToolbarWrapper.inputFields();
    private cssLocator = new CSSLocator();

    public createReference(title: string, url: string, id: string) {
        return this.headerButtons.createReference.click()
            .then(() => browser.wait(this.homePage.isReferenceCreateFormLoaded(), this.waitUpTo.twentySeconds))
            .then(() => this.inputFields.referenceDocId.sendKeys(id))
            .then(() => this.inputFields.referenceDocTitle.sendKeys(title))
            .then(() => this.inputFields.referenceDocUrl.sendKeys(url))
            .then(() => this.headerButtons.editSaveReference.click());
    }

    public goToSpecificItSystemReferences(name: string) {
        return this.itSystemCatalogPage.getPage()
            .then(() => browser.wait(this.itSystemCatalogPage.waitForKendoGrid(), this.waitUpTo.twentySeconds))
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]')).click())
            .then(() => browser.wait(element(this.cssLocator.byDataElementType("organizationButton")),this.waitUpTo.twentySeconds))
            .then(() => element(this.cssLocator.byDataElementType("ReferenceTabButton")).click());
    }

    public getReferenceIdField() {
        return this.inputFields.referenceDocId;
    }
    public getReferenceUrlField() {
        return this.inputFields.referenceDocUrl;
    }
    public getReferenceTitleField() {
        return this.inputFields.referenceDocTitle;
    }
    public getReferenceSaveEditbutton() {
        return this.headerButtons.editSaveReference;
    }

    public getReferenceId(refName: string) {
        return element(by.xpath(`//*/tbody/*/td/a[text()="${refName}"]/parent::*/parent::*//td[@data-element-type="referenceIdObject"]`));
    }

    public getEditButtonFromReference(refName: string) {
        return element(by.xpath(`//*/tbody/*/td/a[text()="${refName}"]/parent::*/parent::*//*/button[@data-element-type="editReference"]`));
    }

    public getDeleteButtonFromReference(refName: string) {
        return element(by.xpath(`//*/tbody/*/td/a[text()="${refName}"]/parent::*/parent::*//*/button[@data-element-type="deleteReference"]`));
    }

    public getDeleteButtonFromReferenceWithInvalidUrl(refName: string) {
        return element(by.xpath(`//*/tbody/*/td[contains(text(),${refName})]/parent::*/parent::*//*/button[@data-element-type="deleteReference"]`));
    }

    public getUrlFromReference(refName: string) {
        return element(by.xpath(`//*/tbody/*/td/a[text()="${refName}"]/parent::*/parent::*//td[@data-element-type="referenceObject"]/a`));
    }
}
export = ReferenceHelper;