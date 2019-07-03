import IPageObject = require("../../object-wrappers/IPageObject.po");
import kendoToolbarWrapper = require("../../object-wrappers/kendoToolbarWrapper");

class ItSystemEditPo extends kendoToolbarWrapper implements IPageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/overview");
    }


    kendoColumnHeader = this.mainGridElement.all(by.css("th.k-header.ng-scope.k-with-icon"));
    columnHeaderValidity = this.kendoColumnHeader.get(0);
    columnLocalSystemID = this.kendoColumnHeader.get(1);
    columnSystemUUID = this.kendoColumnHeader.get(2);
    columnParentSystem = this.kendoColumnHeader.get(3);
    columnSystemName = this.kendoColumnHeader.get(4);
    columnSystemVersion = this.kendoColumnHeader.get(5);
    columnLocalCallName = this.kendoColumnHeader.get(6);
    columnResponsibleOrganization = this.kendoColumnHeader.get(7);
    columnBusinessType = this.kendoColumnHeader.get(8);
    columnApplicationType = this.kendoColumnHeader.get(9);
    columnKLEID = this.kendoColumnHeader.get(10);
    columnKLEName = this.kendoColumnHeader.get(11);
    columnReference = this.kendoColumnHeader.get(12);
    columnExternalReference = this.kendoColumnHeader.get(13);
    columnDataType = this.kendoColumnHeader.get(14);
    columnContract = this.kendoColumnHeader.get(15);
    columnSupplier = this.kendoColumnHeader.get(16);
    columnProject = this.kendoColumnHeader.get(17);
    columnUsedBy = this.kendoColumnHeader.get(18);
    columnLastEditedUser = this.kendoColumnHeader.get(19);
    columnLastEditedDate = this.kendoColumnHeader.get(20);
    columnDateOfUse = this.kendoColumnHeader.get(21);
    columnArchiveDuty = this.kendoColumnHeader.get(22);
    columnHoldsDocument = this.kendoColumnHeader.get(23);
    columnEndDate = this.kendoColumnHeader.get(24);
    columnRiskEvaluation = this.kendoColumnHeader.get(25);
    columnUrlListing = this.kendoColumnHeader.get(26);
    columnDataProcessingAgreement = this.kendoColumnHeader.get(27);

}

export = ItSystemEditPo;