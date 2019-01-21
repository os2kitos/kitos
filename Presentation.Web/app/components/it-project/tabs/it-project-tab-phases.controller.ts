module Kitos.ItProject.Edit {
    "use strict";

    export interface IPhasesController {
        project: any;
        datepickerOptions: IDatepickerOptions;

        updatePhaseDate(phase: IPhase, index: number): void;
        updatePhaseName(phase: IPhase, index: number): void;
        updateSelectedPhase(index: number): void;
    }

    class PhasesController implements IPhasesController {
        public datepickerOptions: IDatepickerOptions;

        public static $inject: Array<string> = [
            "$scope",
            "$http",
            "notify",
            "project",
            "user"
        ];

        constructor(
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private notify,
            public project,
            private user) {

            this.project.updateUrl = `api/itproject/${this.project.id}?organizationId=${this.user.currentOrganizationId}`;

            this.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            // setup phases
            this.project.phases = [this.project.phase1, this.project.phase2, this.project.phase3, this.project.phase4, this.project.phase5];
        }

        updatePhaseName(phase: IPhase, num: number): void {
            var payload = {};
            payload["Name"] = phase.name;
            this.$http.post(this.project.updateUrl + "&phaseNum=" + num, payload)
                .then(
                    () => this.notify.addSuccessMessage("Feltet er opdateret"),
                    () => this.notify.addErrorMessage("Fejl!")
                );
        }

        updateSelectedPhase(phaseNum: number): void {
            this.patch(this.project.updateUrl, "currentPhase", phaseNum)
                .then(
                    result => {
                        // todo: No evaluation on result. Might be 404 or 500 ?
                        this.notify.addSuccessMessage("Feltet er opdateret");
                        this.project.currentPhase = phaseNum;
                    },
                    () => this.notify.addErrorMessage("Fejl!")
                );
        }

        updatePhaseDate(phase: IPhase, num: number): void {
            var dateObject = moment(phase.startDate, "DD-MM-YYYY");
            var startDate;
            if (phase.startDate === "") {
                startDate = null;
            } else if (dateObject.isValid()) {
                startDate = dateObject.format("YYYY-MM-DD");
            } else {
                this.notify.addErrorMessage("Den indtastede dato er ugyldig.");
                return;
            }
            // update start date of the current phase
            var firstPayload = {};
            firstPayload["StartDate"] = startDate;
            this.$http.post(this.project.updateUrl + "&phaseNum=" + num, firstPayload)
                .then(
                    () => {
                        if (num > 1) {
                            var prevPhaseNum = num - 1;
                            var secondPayload = {};
                            secondPayload["EndDate"] = startDate;
                            // also update end date of the previous phase
                            this.$http.post(this.project.updateUrl + "&phaseNum=" + prevPhaseNum, secondPayload)
                                .then(
                                    () => this.notify.addSuccessMessage("Feltet er opdateret"),
                                    () => this.notify.addErrorMessage("Fejl!"));
                        }
                    },
                    () => this.notify.addErrorMessage("Fejl!")
                );
        }

        private patch(url: string, field: string, value: any) {
            var payload = {};
            payload[field] = value;

            return this.$http({
                method: "PATCH",
                url: url,
                data: payload
            });
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-project.edit.phases", {
                    url: "/phases",
                    templateUrl: "app/components/it-project/tabs/it-project-tab-phases.view.html",
                controller: PhasesController,
                    controllerAs: "projectPhasesVm"
        });
            }
        ]);
}
