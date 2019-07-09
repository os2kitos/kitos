import IPageObject = require("../../object-wrappers/IPageObject.po");
import kendoToolbarWrapper = require("../../object-wrappers/kendoToolbarWrapper");
import navigationBarWrapper = require("../../object-wrappers/navigationBarWrapper");
import subNavigationBarWrapper = require("../../object-wrappers/subNavigationBarWrapper");


class ItSystemEditPo extends kendoToolbarWrapper implements IPageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/overview");
    }

    kendoToolbarWrapper = new kendoToolbarWrapper();
    navigationBarWrapper = new navigationBarWrapper();
    subNavigationBarWrapper = new subNavigationBarWrapper();

    kendoColumnHeaderFilter = this.kendoToolbarWrapper.mainGridElement.all(by.css("tr.k-filter-row"));
    columnHeaderValidityFilter = this.kendoColumnHeaderFilter.get(0);
    columnLocalSystemIDFilter = this.kendoColumnHeaderFilter.get(1);
    columnSystemUUIDFilter = this.kendoColumnHeaderFilter.get(2);
    columnParentSystemFilter = this.kendoColumnHeaderFilter.get(3);
    columnSystemNameFilter = this.kendoColumnHeaderFilter.get(4);
    columnSystemVersionFilter = this.kendoColumnHeaderFilter.get(5);
    columnLocalCallNameFilter = this.kendoColumnHeaderFilter.get(6);
    columnResponsibleOrganizationFilter = this.kendoColumnHeaderFilter.get(7);
    columnBusinessTypeFilter = this.kendoColumnHeaderFilter.get(8);
    columnApplicationTypeFilter = this.kendoColumnHeaderFilter.get(9);
    columnKLEIDFilter = this.kendoColumnHeaderFilter.get(10);
    columnKLENameFilter = this.kendoColumnHeaderFilter.get(11);
    columnReferenceFilter = this.kendoColumnHeaderFilter.get(12);
    columnExternalReferenceFilter = this.kendoColumnHeaderFilter.get(13);
    columnDataTypeFilter = this.kendoColumnHeaderFilter.get(14);
    columnContractFilter = this.kendoColumnHeaderFilter.get(15);
    columnSupplierFilter = this.kendoColumnHeaderFilter.get(16);
    columnProjectFilter = this.kendoColumnHeaderFilter.get(17);
    columnUsedByFilter = this.kendoColumnHeaderFilter.get(18);
    columnLastEditedUserFilter = this.kendoColumnHeaderFilter.get(19);
    columnLastEditedDateFilter = this.kendoColumnHeaderFilter.get(20);
    columnDateOfUseFilter = this.kendoColumnHeaderFilter.get(21);
    columnArchiveDutyFilter = this.kendoColumnHeaderFilter.get(22);
    columnHoldsDocumentFilter = this.kendoColumnHeaderFilter.get(23);
    columnEndDateFilter = this.kendoColumnHeaderFilter.get(24);
    columnRiskEvaluationFilter = this.kendoColumnHeaderFilter.get(25);
    columnUrlListingFilter = this.kendoColumnHeaderFilter.get(26);
    columnDataProcessingAgreementFilter = this.kendoColumnHeaderFilter.get(27);


}

export = ItSystemEditPo;