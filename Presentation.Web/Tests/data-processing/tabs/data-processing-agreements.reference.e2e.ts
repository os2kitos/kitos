import login = require("../../Helpers/LoginHelper");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingHelper = require("../../Helpers/DataProcessingAgreementHelper")
import DataProcessingAgreementEditReferencePageObject =
require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.reference.po");

describe("Data Processing agreement reference test ",
    () => {
        var loginHelper = new login();
        var pageObjectReference = new DataProcessingAgreementEditReferencePageObject();
        var refHelper = new ReferenceHelper();
        var testFixture = new TestFixtureWrapper();
        var dpaHelper = DataProcessingHelper;


        beforeAll(() => {
            loginHelper.loginAsLocalAdmin();
            testFixture.enableLongRunningTest();
            dpaHelper.checkAndEnableDpaModule();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Add, edit and delete reference to a agreement", () => {
            var referenceName = createReferenceName();
            var referenceId = createReferenceId();
            var validUrl = generateValidUrl();
            var agreementName = createAgreementName();
            var invalidUrl = generateInvalidUrl();

            //Creating DPA
            dpaHelper.createDataProcessingAgreement(agreementName)
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(agreementName))
                .then(() => pageObjectReference.getDpaReferenceTabButton().click())
            // creating reference
                .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                .then(() => expect(refHelper.getReferenceId(referenceName).getText()).toEqual(referenceId))
                .then(() => expect(refHelper.getUrlFromReference(referenceName).getAttribute("href")).toEqual(validUrl))
            // changing to invalid URL
                .then(() => editReferenceUrl(referenceName, invalidUrl))
                .then(() => expect(refHelper.getUrlFromReference(referenceName).isPresent()).toBeFalsy())
            // deleting reference
                .then(() => deleteReferenceFromDpa(referenceName))
                .then(() => expect(refHelper.getReferenceId(referenceName).isPresent()).toBeFalse());
        });

        function editReferenceUrl(reference: string, url: string) {

            console.log(`Editing ${reference} url value to ${url}`);
            refHelper.getEditButtonFromReference(reference).click()
                .then(() => refHelper.getReferenceUrlField().clear())
                .then(() => refHelper.getReferenceUrlField().sendKeys(url))
                .then(() => refHelper.getReferenceSaveEditButton().click());
        }

        function deleteReferenceFromDpa(reference: string) {

            console.log(`Deleting ${reference}`);
            refHelper.getDeleteButtonFromReferenceWithInvalidUrl(reference).click()
                .then(() => browser.switchTo().alert().accept());
        }


        function createAgreementName() {
            return `DpaRefTest${new Date().getTime()}`;
        }

        function createReferenceName() {
            return `Reference${new Date().getTime()}`;
        }

        function createReferenceId() {
            return `ID${new Date().getTime()}`;
        }

        function generateValidUrl() {
            return `http://www.${new Date().getTime()}.dk/`;
        }

        function generateInvalidUrl() {
            return `${new Date().getTime()}.dk/`;
        }
    });




