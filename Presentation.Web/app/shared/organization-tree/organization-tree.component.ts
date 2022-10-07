module Kitos.Shared.Components.Progress {
    "use strict";

    //TODO: Move to another the models folder
    export interface IOrganizationTreeComponentOptions {
        nodes: any //TODO
        availableLevels: number | null
        dragDropAllowed: boolean | null
        hasWriteAccess: boolean | null
        selectedNodeChanged?: (node: any | null) => void //TODO: Define the node    
    }

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                options: "<"
            },
            controller: OrganizationTreeComponentController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/progress-spinner/progress-spinner.view.html`
        };
    }

    interface IOrganizationTreeComponentController extends ng.IComponentController {
        options: IOrganizationTreeComponentOptions
    }

    class OrganizationTreeComponentController implements IOrganizationTreeComponentController {
        options: IOrganizationTreeComponentOptions;
        chosenNode: any | null = null; //TODO: Define the node

        $onInit() {
            if (!this.options) {
                console.error("Missing options for OrganizationTreeComponentController");
            } else {
                //TODO: Build the tree
                //TODO: Register the callbacks
            }
        }

        chooseOrgUnit(node, event) {
            //TODO: Event filtering based on "event"
            const callback = this.options.selectedNodeChanged;
            if (this.chosenNode !== node) {
                this.chosenNode = node;
                if (callback != undefined) {
                    //TODO: Selection filtering
                    callback(node);
                }
            }
        }
    }
    angular.module("app")
        .component("organizationTree", setupComponent());
}