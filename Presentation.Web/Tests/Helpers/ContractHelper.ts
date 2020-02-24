import ContractPage = require("../PageObjects/It-contract/ItContractOverview.po");
import ContractTimePage = require("../PageObjects/It-contract/ContractTimeOverview.po");
import CssHelper = require("../Object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import WaitTimers = require("../Utility/WaitTimers");

class ContractHelper {

    private static contractPage = new ContractPage();
    private static contractTimePage = new ContractTimePage();
    private static cssHelper = new CssHelper();
    private static consts = new Constants();
    private static waitUpTo = new WaitTimers();

    public static createContract(name: string) {
        console.log(`Creating contract with name: ${name}`);
        return this.contractPage.getPage()
            .then(() => this.waitForEconomyPageKendoGrid())
            .then(() => this.contractPage.getCreateContractButton().click())
            .then(() => expect(this.contractPage.getContractNameInputField().isPresent()))
            .then(() => this.contractPage.getContractNameInputField().sendKeys(name))
            .then(() => this.contractPage.getSaveContractButton().click())
            .then(() => console.log("Contract created"));
    }

    public static openContract(name: string) {
        console.log(`open details for contract: ${name}`);
        return this.contractPage.getPage()
            .then(() => this.waitForEconomyPageKendoGrid())
            .then(() => this.findCatalogColumnsFor(name).first().click());
    }

    public static getRelationCountFromContractName(name: string) {
        return this.contractTimePage.getPage()
            .then(() => this.waitForTimePageKendoGrid())
            .then(() => {
                const filteredRows = this.findCatalogColumnsFor(name);
                return filteredRows.first().element(by.xpath("../.."))
                    .element(this.cssHelper.byDataElementType(this.consts.kendoRelationCountObject)).getText();
            });
    }

    public static findCatalogColumnsFor(name: string) {
        return this.contractPage.kendoToolbarWrapper.getFilteredColumnElement(this.contractPage.kendoToolbarWrapper.columnObjects().contractName, name);
    }

    public static waitForTimePageKendoGrid() {
        browser.wait(this.contractTimePage.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }

    public static waitForEconomyPageKendoGrid() {
        browser.wait(this.contractTimePage.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }
}

export = ContractHelper;