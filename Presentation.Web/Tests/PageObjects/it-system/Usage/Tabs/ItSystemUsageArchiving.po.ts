import Select2Helper = require("../../../../Helpers/Select2Helper");
import SelectHelper = require("../../../../Helpers/SelectHelper");

class ItSystemUsageArchiving {

    pageIds = {
        archiveDutySelection: "s2id_archiveDuty",
        archiveType: "s2id_archive",
        archiveLocation: "s2id_archiveLocation",
        archiveSupplier: "s2id_archiveSupplier",
        archiveNotes: "ArchiveNotes",
        ArchiveFrequency: "ArchiveFreq",
        registerType: "Registertype",
        archiveFromSystemYesRadioButton : "ArchiveFromSystemYes"
    }

    selectArchiveDuty(selection: string) {
        console.log(`Selecting archive duty '${selection}'`);
        return Select2Helper.selectWithNoSearch(selection, this.pageIds.archiveDutySelection);
    }

    selectArchiveType(selection: string) {
        console.log(`Selecting archive type '${selection}'`);
        return Select2Helper.selectWithNoSearch(selection, this.pageIds.archiveType);
    }

    selectArchiveLocation(selection: string) {
        console.log(`Selecting archive location '${selection}'`);
        return Select2Helper.selectWithNoSearch(selection, this.pageIds.archiveLocation);
    }

    selectArchiveSupplier(selection: string) {
        console.log(`Selecting archive supplier '${selection}'`);
        return Select2Helper.select(selection, this.pageIds.archiveSupplier);
    }

    getArchiveTypeSelection = () => element(by.id(this.pageIds.archiveType));

    getArchiveLocationSelection = () => element(by.id(this.pageIds.archiveLocation));

    getArchiveSupplierSelection = () => element(by.id(this.pageIds.archiveSupplier));

    getArchiveNotesInput = () => element(by.id(this.pageIds.archiveNotes));

    getArchiveFrequencyInput = () => element(by.id(this.pageIds.ArchiveFrequency));

    getRegisterTypeCheckbox = () => element(by.id(this.pageIds.registerType));

    getArchiveFromSystemYesRadioButton = () => element(by.id(this.pageIds.archiveFromSystemYesRadioButton));
}

export = ItSystemUsageArchiving;