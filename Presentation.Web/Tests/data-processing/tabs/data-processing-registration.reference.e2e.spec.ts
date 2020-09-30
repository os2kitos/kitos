import login = require("../../Helpers/LoginHelper");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingHelper = require("../../Helpers/DataProcessingRegistrationHelper")
import DataProcessingRegistrationEditReferencePageObject =
require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.reference.po");

describe("Data Processing registration reference test ",
    () => {
        var loginHelper = new login();
        var pageObjectReference = new DataProcessingRegistrationEditReferencePageObject();
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

        it("Add, edit and delete reference to a registration", () => {
            var referenceName = createReferenceName();
            var referenceId = createReferenceId();
            var validUrl = generateValidUrl();
            var registrationName = createRegistrationName();
            var invalidUrl = generateInvalidUrl();

            //Creating DPR
            dpaHelper.createAndOpenDataProcessingRegistration(registrationName)
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


        function createRegistrationName() {
            return `DprRefTest${new Date().getTime()}`;
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




