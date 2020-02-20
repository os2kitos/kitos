import ContractPage = require("../PageObjects/It-contract/ItContractOverview.po");

class ContractHelper {

    private static contractPage = new ContractPage();

    public static createContract(name: string) {
        console.log(`Creating contract with name: ${name}`);
        return this.contractPage.getPage()
            .then(() => this.contractPage.waitForKendoGrid())
            .then(() => this.contractPage.getCreateContractButton().click())
            .then(() => expect(this.contractPage.getContractNameInputField().isPresent()))
            .then(() => this.contractPage.getContractNameInputField().sendKeys(name))
            .then(() => this.contractPage.getSaveContractButton().click())
            .then(() => console.log("Contract created"));
    }
}

export = ContractHelper;