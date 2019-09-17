import RefePage = require("../PageObjects/It-system/Tabs/ItSystemReference.po");
import WaitTimers = require("../Utility/WaitTimers");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import ItSystemCatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");

class ReferenceHelper {
    private homePage = new RefePage();
    private itSystemCatalogPage = new ItSystemCatalogPage();
    private waitUpTo = new WaitTimers();
    private headerButtons = new RefePage().kendoToolbarWrapper.headerButtons();
    private inputFields = new RefePage().kendoToolbarWrapper.inputFields();
    private cssLocator = new CSSLocator();

    public createReference(title: string, url: string, id: string) {
        return this.headerButtons.createReference.click()
            .then(() => browser.wait(this.homePage.isReferenceCreateFormLoaded(), this.waitUpTo.twentySeconds))
            .then(() => this.inputFields.referenceDocId.sendKeys(id))
            .then(() => this.inputFields.referenceDocTitle.sendKeys(title))
            .then(() => this.inputFields.referenceDocUrl.sendKeys(url))
            .then(() => this.headerButtons.editSaveReference.click());
    }

    public deleteReference(id: string) {
        return this.homePage.getPage()
            .then(() => browser.wait(this.homePage.isCreateReferenceLoaded(), this.waitUpTo.twentySeconds))
            .then(() => {
                element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
                    ele.all(by.tagName("td")).each((tdele) => {

                        tdele.getText().then(val => {

                            if (val === id) {
                                return ele.element(this.cssLocator.byDataElementType("deleteReference")).click()
                                    .then(() => browser.switchTo().alert().accept());
                            }
                        });
                    });
                });
            });
    }

    public goToSpecificItSystemReferences(name: string) {
        return this.itSystemCatalogPage.getPage()
            .then(() => browser.wait(this.itSystemCatalogPage.waitForKendoGrid(), this.waitUpTo.twentySeconds))
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]')).click())
            .then(() => browser.wait(element(this.cssLocator.byDataElementType("organizationButton")),
                this.waitUpTo.twentySeconds))
            .then(() => element(this.cssLocator.byDataElementType("ReferenceTabButton")).click());

    }
}

export = ReferenceHelper;