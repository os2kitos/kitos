module Kitos.Services.UICustomization {

    export interface IUICustomizationService {
        /**
         * Loads the currently active UI customization configuration based on the config blueprint as well as any local adjustments
         * @param module
         */
        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI>;
        /**
         * Saves the active configuration for the current organization
         * @param config
         */
        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): ng.IPromise<void>;
    }

    class CollectNodeStatesFromUiCustomizationTreeVisitor implements Models.UICustomization.IUICustomizationTreeVisitor {
        private readonly _nodes: Array<Models.Api.UICustomization.ICustomizedUINodeDTO> = [];

        get nodes() {
            return [...this._nodes];
        }

        visitNode(node: Models.UICustomization.UINode) {
            this._nodes.push({
                enabled: node.available,
                fullKey: node.key
            });
        }

        visitModule(node: Models.UICustomization.CustomizedModuleUI) { /* We don't care much for modules - we care about the nodes */ }
    }

    class UICustomizationService implements IUICustomizationService {

        static $inject = ["genericApiWrapper", "userService", "uiCustomizationStateCache"];

        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly userService: Services.IUserService,
            private readonly uiCustomizationStateCache: UiCustomizationStateCache) { }

        private loadBluePrint(module: Models.UICustomization.CustomizableKitosModule): Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint {

            switch (module) {
                case Models.UICustomization.CustomizableKitosModule.ItSystemUsage:
                    return Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;
                default:
                    throw `Unknown module blueprint:${module}`;
            }
        }

        private loadPersistedPreferences(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.Api.UICustomization.IUIModuleCustomizationDTO> {
            return this
                .userService
                .getUser()
                .then(user => this.genericApiWrapper.getDataFromUrl<Models.Api.UICustomization.IUIModuleCustomizationDTO>(`/api/v1/organizations/${user.currentOrganizationId}/ui-config/modules/${module}`)
                    .then(
                        response => response,
                        error => {
                            if (error !== Models.Api.ApiResponseErrorCategory.NotFound) {
                                console.error("Error loading module config for module ", module, " in organization ", user.currentOrganizationId, ". Failed with:", error);
                            }
                            return <Models.Api.UICustomization.IUIModuleCustomizationDTO>{
                                module: module,
                                nodes: [],
                                organizationId: user.currentOrganizationId
                            }
                        }
                    )
                );
        }

        private buildActiveConfiguration(bluePrint: Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint, persistedPreferences: Models.Api.UICustomization.IUIModuleCustomizationDTO): Models.UICustomization.ICustomizedModuleUI {
            //Create a lookup to define the initial state of nodes as they were saved by the local administrator
            const persistedConfigLookup: Record<string, boolean> = persistedPreferences.nodes.reduce((currentMap, node) => {
                currentMap[node.fullKey] = node.enabled;
                return currentMap;
            }, {});


            function buildChildren(currentNodeBluePrint: Models.UICustomization.Configs.ICustomizableUINodeConfig, parentAvailable: boolean): Array<Models.UICustomization.IUINode> {
                const children: Array<Models.UICustomization.IUINode> = [];
                const childrenObj = currentNodeBluePrint.children;
                if (childrenObj != undefined) {
                    for (let childKey of Object.keys(childrenObj)) {
                        const bluePrint: Models.UICustomization.Configs.ICustomizableUINodeConfig = currentNodeBluePrint.children[childKey];
                        const serverConfig = persistedConfigLookup[bluePrint.fullKey];
                        const available = serverConfig != undefined ? serverConfig : true;
                        children.push(new Models.UICustomization.UINode
                            (
                                bluePrint.fullKey,                      //key
                                !bluePrint.readOnly && parentAvailable, //editable
                                available,                              //available state
                                bluePrint.readOnly,                     //readonly
                                buildChildren(bluePrint, available),    //build children recursively,
                                bluePrint.helpText                      //help text for the local admin
                            )
                        );
                    }
                }
                return children;
            }

            return new Models.UICustomization.CustomizedModuleUI(bluePrint.module, new Models.UICustomization.UINode(bluePrint.module, false, true, true, buildChildren(bluePrint, true)));
        }

        loadActiveConfiguration(module: Models.UICustomization.CustomizableKitosModule): ng.IPromise<Models.UICustomization.ICustomizedModuleUI> {
            let persisted: angular.IPromise<Models.Api.UICustomization.IUIModuleCustomizationDTO>;
            let bluePrint: Models.UICustomization.Configs.ICustomizableUIModuleConfigBluePrint;

            switch (module) {
                case Models.UICustomization.CustomizableKitosModule.ItSystemUsage:
                    {
                        persisted = this.loadPersistedPreferences(module);
                        bluePrint = this.loadBluePrint(module);
                        break;
                    }
                default:
                    throw `Unknown module:${module}`;
            }
            return persisted
                .then(config => this.buildActiveConfiguration(bluePrint, config))
                .then(activeConfig => {
                    //update the shared cache with the latest data
                    this.uiCustomizationStateCache[module] = activeConfig;
                    return activeConfig;
                });
        }

        saveActiveConfiguration(config: Models.UICustomization.ICustomizedModuleUI): ng.IPromise<void> {
            return this
                .userService
                .getUser()
                .then(user => {

                    const nodeCollectionVisitor = new CollectNodeStatesFromUiCustomizationTreeVisitor();
                    config.accept(nodeCollectionVisitor);

                    const dto: Models.Api.UICustomization.IUIModuleCustomizationDTO = { nodes: nodeCollectionVisitor.nodes };
                    return this
                        .genericApiWrapper.put(`/api/v1/organizations/${user.currentOrganizationId}/ui-config/modules/${config.module}`, dto)
                        .then(_ => {
                            //Update the shared application cache
                            this.uiCustomizationStateCache[config.module] = config;
                        });
                });
        }
    }

    app.service("uiCustomizationService", UICustomizationService);
}