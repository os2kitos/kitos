module Kitos.DataProcessing.Registration.Edit.Oversight.Modal {
    "use strict";

    export class OversightModalController {
        static $inject: Array<string> = [
            "hasWriteAccess",
            "datepickerOptions",
            "submitFunction",
            "mainController",
            "oversightId",
            "oversightDate",
            "oversightRemark",
            "modalType",
            "$uibModalInstance"
        ];

        constructor(
            public hasWriteAccess,
            public datepickerOptions,
            public submitFunction,
            public mainController: EditOversightDataProcessingRegistrationController,
            public oversightId: number,
            public oversightDate: string,
            public oversightRemark: string,
            public modalType: ModalType,
            public $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance
        ) {
        }

        save() {
            var result = null;
            if (this.modalType === ModalType.create) {
                result = this.submitFunction(this.mainController, this.oversightDate, this.oversightRemark);
            }
            if (this.modalType === ModalType.modify) {
                result = this.submitFunction(this.mainController, this.oversightId, this.oversightDate, this.oversightRemark);
            }
            if (result != null) {
                this.$uibModalInstance.close();
            }
        }

    }

    export enum ModalType {
        create = 0,
        modify = 1
    }
}
