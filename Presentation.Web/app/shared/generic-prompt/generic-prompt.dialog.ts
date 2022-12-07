module Kitos.Shared.Generic.Prompt {

    export enum GenericCommandCategory {
        Success = "success",
        Primary = "primary",
        Warning = "warning",
        Danger = "danger"
    }

    export interface GenericPromptCommand<T> {
        category: GenericCommandCategory
        text: string
        value: T
    }

    export interface GenericPromptConfig<T> {
        title: string | null
        body?: string
        bodyTemplatePath?: string
        includeStandardCancelButton?: boolean
        commands: Array<GenericPromptCommand<T>>
    }

    export interface IGenericPromptFactory {
        open<T>(config: GenericPromptConfig<T>): ng.ui.bootstrap.IModalInstanceService
    }

    export class GenericPromptFactory implements IGenericPromptFactory {
        static $inject = ["$uibModal"];
        constructor(private readonly $uibModal: ng.ui.bootstrap.IModalService) { }

        open<T>(config: GenericPromptConfig<T>): ng.ui.bootstrap.IModalInstanceService {
            return this.$uibModal.open({
                windowClass: "modal fade in",
                templateUrl: "app/shared/generic-prompt/generic-prompt.view.html",
                controller: GenericPromptController,
                controllerAs: "vm",
                resolve: {
                    "genericPromptConfig": [() => config],
                },
                backdrop: "static", //Make sure accidental click outside the modal does not close it during the import process
            });
        }
    }

    export interface GenericPromptCommandButton {
        category: GenericCommandCategory
        text: string
        handle: () => void
    }

    class GenericPromptController {
        static $inject = ["$uibModalInstance", "genericPromptConfig"];
        readonly buttons: GenericPromptCommandButton[] = [];
        title: string | null = null;
        body: string | null = null;
        bodyTemplatePath: string | null = null;

        constructor(
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly genericPromptConfig: GenericPromptConfig<unknown>) {
        }

        $onInit() {
            if (!this.genericPromptConfig) {
                console.error("Missing prompt config");
            } else {
                this.title = this.genericPromptConfig.title ?? null;
                this.body = this.genericPromptConfig.body ?? null;
                this.bodyTemplatePath = this.genericPromptConfig.bodyTemplatePath ?? null;

                //Add custom commands
                for (var command of this.genericPromptConfig.commands) {
                    const buttonCommand = command;
                    this.addButton(command.category, command.text, () => this.closeDialog(buttonCommand));
                }

                //Add standard cancel if requested
                if (this.genericPromptConfig.includeStandardCancelButton) {
                    this.addButton(GenericCommandCategory.Warning, "Annuller", () => this.cancel());
                }
            }
        }

        private addButton(category: GenericCommandCategory, text: string, handle: () => void) {
            this.buttons.push({
                category: category,
                text: text,
                handle: handle
            });
        }

        cancel() {
            this.$uibModalInstance.dismiss();
        }

        private closeDialog(command: GenericPromptCommand<unknown>) {
            this.$uibModalInstance.close(command.value);
        }
    }

    app.service("genericPromptFactory", GenericPromptFactory)
}