module Kitos.ItProject.Edit {
    interface IDescriptionController {
        project: any;
    }

    class DescriptionController implements IDescriptionController {
        static $inject: Array<string> = ["project"];
        constructor(public project) {
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("it-project.edit.description", {
                    url: "/description",
                    templateUrl: "app/components/it-project/tabs/it-project-tab-description.view.html",
                    controller: DescriptionController,
                    controllerAs: "projectDescriptionVm"
                });
            }
        ]);
}
