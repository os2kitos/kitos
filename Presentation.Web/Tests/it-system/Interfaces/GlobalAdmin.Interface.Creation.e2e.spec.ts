import Login = require("../../Helpers/LoginHelper");
import ItSystemInterface = require("../../PageObjects/It-system/Interfaces/itSystemInterface.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import InterfaceCatalogHelper = require("../../Helpers/InterfaceCatalogHelper");
import InterfaceHelper = require("../../Helpers/InterfaceHelper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");
import constant = require("../../Utility/Constants");

describe("Only Global Administrator is able to create and fill out an interface", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemInterface();
    var testFixture = new TestFixtureWrapper();
    var constants = new constant();

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Global Admin is able to create and fill out data on a interface", () => {
        var interfaceName = createInterfaceName();
        var id = createString();
        var description = createString();
        var descriptionLink = createString();
        var note = createString();
        var dataRow = createString();
        var systemName = createItSystem();
        var systemInterface = "CSV";
        var access = "Lokal";
        var org = "Fælles Kommune";
        var dataType = "Sag";
        var version = createVersion();

        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                console.log("Getting page");
                return pageObject.getPage();
            }).then(() => {
                console.log("Creating interface");
                return InterfaceCatalogHelper.createInterface(interfaceName);
            }).then(() => {
                console.log("Creating IT system");
                return ItSystemHelper.createSystem(systemName);
            }).then(() => {
                console.log("Inserting data");
                return writeDataToAllInputs(interfaceName, id, description, descriptionLink, note, dataRow, systemName, systemInterface, access, dataType, version);
            }).then(() => {
                console.log("Verifying data");
                return verifyDataWasSaved(interfaceName, id, description, descriptionLink, note, dataRow, systemName, systemInterface, access, dataType, version);
            }).then(() => {
                console.log("Verifying BelongsTo for interface is same as its exhibit system and field is readonly");
                verifyAttributeIsEqualTo(pageObject.getInterfaceBelongsToField(), "disabled", "true");
                verifyAttributeIsEqualTo(pageObject.getInterfaceBelongsToField(), "value" , org);
            });
    });

    function writeDataToAllInputs(
        nameOfInterface: string,
        id: string,
        description: string,
        descriptionLink: string,
        note: string,
        dataRow: string,
        systemName: string,
        systemInterface: string,
        access: string,
        dataTypeTable: string,
        version: string) {
        return InterfaceCatalogHelper.gotoSpecificInterface(nameOfInterface)
            .then(() => InterfaceHelper.writeDataToTextInput(nameOfInterface, constants.interfaceNameInput)
                .then(() => InterfaceHelper.writeDataToTextInput(id, constants.interfaceIdInput))
                .then(() => InterfaceHelper.writeDataToTextInput(description, constants.interfaceDescriptionInput))
                .then(() => InterfaceHelper.writeDataToTextInput(descriptionLink, constants.interfaceDescriptionLinkInput))
                .then(() => InterfaceHelper.writeDataToTextInput(note, constants.interfaceNoteInput))
                .then(() => InterfaceHelper.selectDataFromSelect2Field(systemName, constants.interfaceSelectExhibit))
                .then(() => InterfaceHelper.selectDataFromNoSearchSelect2Field(systemInterface, constants.interfaceSelectInterface))
                .then(() => InterfaceHelper.selectDataFromSelect2Field(access, constants.interfaceSelectAccess))
                .then(() => InterfaceHelper.writeDataToTextInput(version, constants.interfaceVersionInput))
                .then(() => InterfaceHelper.writeDataToTable(dataRow, dataTypeTable)));
    };

    function verifyDataWasSaved(
        nameOfInterface: string,
        id: string,
        description: string,
        descriptionLink: string,
        note: string,
        dataRow: string,
        systemName: string,
        systemInterface: string,
        access: string,
        dataTypeTable: string,
        version: string) {
        return InterfaceCatalogHelper.gotoSpecificInterface(nameOfInterface)
            .then(() => {
                InterfaceHelper.verifyDataFromTextInput(nameOfInterface, constants.interfaceNameInput);
                InterfaceHelper.verifyDataFromTextInput(id, constants.interfaceIdInput);
                InterfaceHelper.verifyDataFromTextInput(description, constants.interfaceDescriptionInput);
                InterfaceHelper.verifyDataFromTextInput(descriptionLink, constants.interfaceDescriptionLinkInput);
                InterfaceHelper.verifyDataFromTextInput(note, constants.interfaceNoteInput);
                InterfaceHelper.verifyDataFromTextInput(version, constants.interfaceVersionInput);
                InterfaceHelper.verifyDataFromTextInput(dataRow, constants.interfaceDataInput);

                InterfaceHelper.verifyDataFromSelect2(systemName, constants.interfaceSelectExhibit);
                InterfaceHelper.verifyDataFromSelect2(systemInterface, constants.interfaceSelectInterface);
                InterfaceHelper.verifyDataFromSelect2(access, constants.interfaceSelectAccess);
                InterfaceHelper.verifyDataFromSelect2(dataTypeTable, constants.interfaceSelectTableDataType);
            });
    };

    function verifyAttributeIsEqualTo(element: protractor.ElementFinder , attributeValueToGet: string, expectedValue: string) {
        expect(element.getAttribute(attributeValueToGet)).toEqual(expectedValue);
    }


    function createInterfaceName() {
        return `Interface${new Date().getTime()}`;
    }

    function createItSystem() {
        return `ItSystem${new Date().getTime()}`;
    }

    function createString() {
        return `Data${new Date().getTime()}`;
    }

    function createVersion() {
        return `${new Date().getTime()}`;
    }
});