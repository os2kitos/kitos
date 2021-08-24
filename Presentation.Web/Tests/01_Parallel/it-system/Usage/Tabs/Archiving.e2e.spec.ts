import login = require("../../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../../../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../../../../Helpers/SideNavigation/LocalItSystemNavigation");
import ItSystemUsageArchiving = require("../../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageArchiving.po");
import Select2Helper = require("../../../../Helpers/Select2Helper");

describe("On the archiving page the user can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
        var itSystemName = `localArchivingTest${new Date().getTime()}`;
        var po = new ItSystemUsageArchiving();

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => ItSystemHelper.createLocalSystem(itSystemName))
                .then(() => SystemUsageHelper.openLocalSystem(itSystemName))
                .then(() => LocalItSystemNavigation.openArchivingPage());
        },
            testFixture.longRunningSetup());

        beforeEach(() => {
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        const validateEditorState = (isEnabled: boolean) => {
            expect(po.getRegisterTypeCheckbox().isEnabled()).toBe(isEnabled);
            expect(po.getArchiveNotesInput().isEnabled()).toBe(isEnabled);
            expect(po.getArchiveFrequencyInput().isEnabled()).toBe(isEnabled);
            Select2Helper.assertIsEnabled(po.getArchiveLocationSelection, isEnabled);
            Select2Helper.assertIsEnabled(po.getArchiveSupplierSelection, isEnabled);
            Select2Helper.assertIsEnabled(po.getArchiveTypeSelection, isEnabled);
        };

        const selectArchiveDuty = (selection: string) => po.selectArchiveDuty(selection);
        const selectArchiveType = (selection: string) => po.selectArchiveType(selection);
        const selectArchivingPlace = (selection: string) => po.selectArchiveLocation(selection);
        const selectArchivingSupplier = (selection: string) => po.selectArchiveSupplier(selection);
        const enterArchivingComment = (newComment: string) => po.getArchiveNotesInput().sendKeys(newComment);
        const setArchivingFrequency = (frequency: number) => po.getArchiveFrequencyInput().sendKeys(frequency.toString());
        const toggleRegisterType = () => po.getRegisterTypeCheckbox().click();
        const selectArchiveFromSystemToYes = () => po.getArchiveFromSystemYesRadioButton().click();

        const validateValues = (
            duty: string,
            type: string,
            place: string,
            supplier: string,
            comment: string,
            frequency: number,
            registerType: boolean,
            archiveFromSystemYesState: boolean) => {
            console.log("Asserting", type, place, supplier, comment, frequency, registerType, archiveFromSystemYesState);
            expect(po.getArchiveNotesInput().getAttribute('value')).toBe(comment);
            expect(po.getArchiveFrequencyInput().getAttribute('value')).toBe(frequency.toString());
            expect(po.getArchiveFromSystemYesRadioButton().isSelected()).toBe(archiveFromSystemYesState);
            expect(po.getRegisterTypeCheckbox().isSelected()).toBe(registerType);
            expect(Select2Helper.getData(po.pageIds.archiveDutySelection).getText()).toEqual(duty);
            expect(Select2Helper.getData(po.pageIds.archiveType).getText()).toEqual(type);
            expect(Select2Helper.getData(po.pageIds.archiveSupplier).getText()).toEqual(supplier);
        }

        it("Edit archiving for local IT-system usage",
            () => {
                const inputs = {
                    undecidedDuty: " ",
                    duty: "B",
                    type: "Arkiveret",
                    place: "Aalborg",
                    supplier: "Fælles Kommune",
                    comment: `testComment${new Date().getTime()}`,
                    frequency: Math.floor(Math.random() * 26) // Range: [0,25]
                }

                return LocalItSystemNavigation.openArchivingPage()
                    .then(() => validateEditorState(false))
                    .then(() => selectArchiveDuty(inputs.duty))
                    .then(() => validateEditorState(true))
                    .then(() => selectArchiveType(inputs.type))
                    .then(() => selectArchivingPlace(inputs.place))
                    .then(() => selectArchivingSupplier(inputs.supplier))
                    .then(() => enterArchivingComment(inputs.comment))
                    .then(() => setArchivingFrequency(inputs.frequency))
                    .then(() => toggleRegisterType())
                    .then(() => selectArchiveFromSystemToYes())
                    .then(() => browser.refresh())
                    .then(() => browser.waitForAngular())
                    .then(() => validateValues(inputs.duty, inputs.type, inputs.place, inputs.supplier, inputs.comment, inputs.frequency, true, true))
                    .then(() => selectArchiveDuty(inputs.undecidedDuty))
                    .then(() => validateEditorState(false));
            });
    });