module Kitos.Shared.Components.Organization {
    "use strict";

    export interface IOrganizationTreeNode {
        name: string
        id: string
        nodes: Array<IOrganizationTreeNode>
    }

    interface IOrganizationTreeNodeViewModel extends IOrganizationTreeNode {
        level: number
    }

    export interface IOrganizationTreeComponentOptions {
        root: IOrganizationTreeNode
        availableLevels: number | null
        selectedNodeChanged?: (node: IOrganizationTreeNode | null) => void
    }

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                options: "<"
            },
            controller: OrganizationTreeComponentController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/organization-tree/organization-tree.view.html`
        };
    }

    interface IOrganizationTreeComponentController extends ng.IComponentController {
        options: IOrganizationTreeComponentOptions
    }

    class OrganizationTreeComponentController implements IOrganizationTreeComponentController {
        options: IOrganizationTreeComponentOptions;
        chosenNode: IOrganizationTreeNodeViewModel | null = null;
        nodesVm: Array<IOrganizationTreeNodeViewModel> | null = null;
        $onInit() {
            if (!this.options) {
                console.error("Missing options for OrganizationTreeComponentController");
            } else {
                this.nodesVm = this.applyLevels([this.options.root], 1);
            }
        }

        /**
         * Applies level information used for quick filtering in the view
         * @param nodes
         * @param level
         */
        private applyLevels(nodes: IOrganizationTreeNode[], level: number): IOrganizationTreeNodeViewModel[] {
            nodes.forEach(node => {
                const vm = node as IOrganizationTreeNodeViewModel;
                vm.level = level;
                this.applyLevels(vm.nodes, level + 1);
            });
            return nodes as IOrganizationTreeNodeViewModel[];
        }

        chooseOrgUnit(node, event) {
            const callback = this.options.selectedNodeChanged;
            if (this.chosenNode !== node) {
                this.chosenNode = node;
                if (callback != undefined) {
                    callback(node);
                }
            }
        }
    }
    angular.module("app")
        .component("organizationTree", setupComponent());
}