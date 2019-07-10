import CSSLocator = require("./CSSLocatorHelper");
import Constants = require("../../app/utility/Constants");

type HeaderButtons = {
    resetFilter: protractor.ElementFinder, saveFilter: protractor.ElementFinder, useFilter: protractor.ElementFinder, deleteFilter: protractor.ElementFinder,
    export: protractor.ElementFinder
};

type ClickableColumnHeaders = {
    localSystemId: protractor.ElementFinder, systemUUID: protractor.ElementFinder, parentSystem: protractor.ElementFinder, systemName: protractor.ElementFinder,
    systemVersion: protractor.ElementFinder, localCallName: protractor.ElementFinder, reponsibleOrganization: protractor.ElementFinder, businessType: protractor.ElementFinder,
    applicationType: protractor.ElementFinder, KLEID: protractor.ElementFinder, KLEName: protractor.ElementFinder, Reference: protractor.ElementFinder,
    externalReference: protractor.ElementFinder, dataType: protractor.ElementFinder, contract: protractor.ElementFinder, supplier: protractor.ElementFinder,
    project: protractor.ElementFinder, usedBy: protractor.ElementFinder, lastEditedBy: protractor.ElementFinder, lastEditedAt: protractor.ElementFinder,
    dateOfUse: protractor.ElementFinder, archiveDuty: protractor.ElementFinder, holdsDocument: protractor.ElementFinder, endDate: protractor.ElementFinder,
    riskEvaluation: protractor.ElementFinder, urlListing: protractor.ElementFinder, dataProcessingAgreement: protractor.ElementFinder,contractName: protractor.ElementFinder
};

type ColumnObjects = {
    localSystemId: protractor.ElementArrayFinder, systemUUID: protractor.ElementArrayFinder, parentSystem: protractor.ElementArrayFinder, systemName: protractor.ElementArrayFinder,
    systemVersion: protractor.ElementArrayFinder, localCallName: protractor.ElementArrayFinder, reponsibleOrganization: protractor.ElementArrayFinder, businessType: protractor.ElementArrayFinder,
    applicationType: protractor.ElementArrayFinder, KLEID: protractor.ElementArrayFinder, KLEName: protractor.ElementArrayFinder, Reference: protractor.ElementArrayFinder,
    externalReference: protractor.ElementArrayFinder, dataType: protractor.ElementArrayFinder, contract: protractor.ElementArrayFinder, supplier: protractor.ElementArrayFinder,
    project: protractor.ElementArrayFinder, usedBy: protractor.ElementArrayFinder, lastEditedBy: protractor.ElementArrayFinder, lastEditedAt: protractor.ElementArrayFinder,
    dateOfUse: protractor.ElementArrayFinder, archiveDuty: protractor.ElementArrayFinder, holdsDocument: protractor.ElementArrayFinder, endDate: protractor.ElementArrayFinder,
    riskEvaluation: protractor.ElementArrayFinder, urlListing: protractor.ElementArrayFinder, dataProcessingAgreement: protractor.ElementArrayFinder, contractName: protractor.ElementArrayFinder,
};

type footerNavigationButtons = {
    firstPage: protractor.ElementFinder, onePageBack: protractor.ElementFinder, onePageForward: protractor.ElementFinder, lastPage: protractor.ElementFinder,
    refresh: protractor.ElementFinder
};


var byHook = new CSSLocator().byDataHook;

class kendoToolbarWrapper {

    public headerButtons(): HeaderButtons {

        // TODO change to use byHook once kendo objects in app are refactored.
        var buttons: HeaderButtons = {
            resetFilter: element(by.buttonText("Nulstil")),
            saveFilter: element(by.buttonText("Gem filter")),
            useFilter: element(by.buttonText("Anvend filter")),
            deleteFilter: element(by.buttonText("Slet filter")),
            export: this.notYetAHook()
        };
        return buttons;
    }

    public columnHeaders(): ClickableColumnHeaders {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ClickableColumnHeaders = {
            localSystemId: this.notYetAHook(),
            systemUUID: this.notYetAHook(),
            parentSystem: this.notYetAHook(),
            systemName: kendo.getColumnHeaderClickable(consts.kendoSystemNameHeader),
            contractName: kendo.getColumnHeaderClickable(consts.kendoContractNameHeader),
            systemVersion: this.notYetAHook(),
            localCallName: this.notYetAHook(),
            reponsibleOrganization: this.notYetAHook(),
            businessType: this.notYetAHook(),
            applicationType: this.notYetAHook(),
            KLEID: this.notYetAHook(),
            KLEName: this.notYetAHook(),
            Reference: this.notYetAHook(),
            externalReference: this.notYetAHook(),
            dataType: this.notYetAHook(),
            contract: this.notYetAHook(),
            supplier: this.notYetAHook(),
            project: this.notYetAHook(),
            usedBy: this.notYetAHook(),
            lastEditedBy: this.notYetAHook(),
            lastEditedAt: this.notYetAHook(),
            dateOfUse: this.notYetAHook(),
            archiveDuty: this.notYetAHook(),
            holdsDocument: this.notYetAHook(),
            endDate: this.notYetAHook(),
            riskEvaluation: this.notYetAHook(),
            urlListing: this.notYetAHook(),
            dataProcessingAgreement: this.notYetAHook()
        };
        return columns;
    }

    public columnObjects(): ColumnObjects {
        var kendo = new kendoHelper();
        var consts = new Constants();

        var columns: ColumnObjects = {
            localSystemId: this.notYetAHook2(),
            systemUUID: this.notYetAHook2(),
            parentSystem: this.notYetAHook2(),
            systemName: kendo.getColumnItemLinks(consts.kendoSystemNameObjects),
            systemVersion: this.notYetAHook2(),
            localCallName: this.notYetAHook2(),
            reponsibleOrganization: this.notYetAHook2(),
            businessType: this.notYetAHook2(),
            applicationType: this.notYetAHook2(),
            KLEID: this.notYetAHook2(),
            KLEName: this.notYetAHook2(),
            Reference: this.notYetAHook2(),
            externalReference: this.notYetAHook2(),
            dataType: this.notYetAHook2(),
            contract: this.notYetAHook2(),
            supplier: this.notYetAHook2(),
            project: this.notYetAHook2(),
            usedBy: this.notYetAHook2(),
            lastEditedBy: this.notYetAHook2(),
            lastEditedAt: this.notYetAHook2(),
            dateOfUse: this.notYetAHook2(),
            archiveDuty: this.notYetAHook2(),
            holdsDocument: this.notYetAHook2(),
            endDate: this.notYetAHook2(),
            riskEvaluation: this.notYetAHook2(),
            urlListing: this.notYetAHook2(),
            dataProcessingAgreement: this.notYetAHook2(),
            contractName: kendo.getColumnItemLinks(consts.kendoContractNameObjects),
            
        };
        return columns;
    }

    public footerNavigationButtons(): footerNavigationButtons {
        var buttons: footerNavigationButtons = {
            firstPage: this.notYetAHook(),
            onePageBack: this.notYetAHook(),
            onePageForward: this.notYetAHook(),
            lastPage: this.notYetAHook(),
            refresh: this.notYetAHook()
        }
        return buttons;
    }
    
    // Need fix how selectors work.

    public roleSelector = null;
    public gridNavigatorSelector = null;

    // Dummy method until every kendo field is filled with a data-hook
    public notYetAHook() {
        return element(null);
    }

    public notYetAHook2() {
        return element.all(null);
    }
}

class kendoHelper {

    // Needed until there's found a way to add a data-hook to the a tag directly
    public getColumnHeaderClickable(headerHook: string) {
        return element(byHook(headerHook)).element(by.css("a[class=k-link]"));
    }

    public getColumnItemLinks(itemHook: string) {
        return element.all(byHook(itemHook)).all(by.tagName("a"));
    }
}

export = kendoToolbarWrapper;
