module Kitos.UICustomization {

    /**
     * Defines the valid customization modules for kitos
     */
    export enum CustomizableKitosModule {
        ItSystemUsage = "it-system-usage"
    }

    /**
     * Defined 
     */
    export interface ICustomizedModuleUI {
        module: CustomizableKitosModule;
        /**
         * Used by the generic directive to check if the node is enabled
         * @param fullKey
         */
        isAvailable(fullKey: string): boolean;
        /**
         * Used by the administration ui to change the state of a node
         * @param fullKey
         * @param newState
         */
        changeAvailableState(fullKey: string, newState: boolean): void;
    }

    export interface IUINode {
        readOnly: boolean;
        helpText: boolean;
        editable: boolean;
        available: boolean;
        key: string;
        children: Array<IUINode>;
    }

    class UINode implements IUINode {
        private readonly _isReadOnly: boolean;
        private readonly _helpText: boolean;
        private readonly _editable: boolean;
        private readonly _available: boolean;
        private readonly _key: string;
        private readonly _children: IUINode[];

        constructor(helpText: boolean, editable: boolean, available: boolean, key: string, children: IUINode[], isReadOnly: boolean) {
            this._isReadOnly = isReadOnly;
            this._children = children;
            this._helpText = helpText;
            this._editable = editable;
            this._available = available;
            this._key = key;

            //TODO: Create a map for children where the key is child.FullKey.removePrefix(this."fullKey")
        }

        get children(): IUINode[] {
            return [...this._children];
        }

        get readOnly() {
            return this._isReadOnly;
        }

        get helpText() {
            return this._helpText;
        }

        get editable() {
            return this._editable;
        }

        get available() {
            return this._available;
        }

        get key() {
            return this._key;
        }
    }


    class CustomizedModuleUI implements ICustomizedModuleUI {
        private readonly _root: IUINode;
        private readonly _module: CustomizableKitosModule;

        constructor(module: CustomizableKitosModule, root: IUINode) {
            this._root = root;
            this._module = module;
        }

        isAvailable(fullKey: string): boolean {
            //TODO :Check the path. If not found, then it is available (not controlled by config and this will be forward compatible). If any parent along the way is not available, the answer is not available. If the leaf is found, return that state
            throw new Error("Not implemented");
        }

        changeAvailableState(fullKey: string, newState: boolean): void {
            //TODO: Fin the node (if any node along the way is non-editable, or disabled, we have problem)
            //TODO: Any child:
            // if newState === true, then all children are enabled and editable, else all children are uneditable
            throw new Error("Not implemented");
        }

        get module() {
            return this._module;
        }

        //TODO: Merge server config into structure?
    }


    //TODO: Config structure could be a typescript object that we post-process and add the full key to each level
    /*
     *
     *    const iSystemConfig: {
     *          enabled: boolean
     *          children: [{
     *              {
     *                  frontPage {
     *                      enabledByDefault: true,
     *                      
     *                  }
     *              }
     *          }]
     *      }
     *
     */

    //TODO: When a parent is set as disabled, then it should disable all children and make them uneditable
    //TODO: ability to apply external config to tree
    //TOOD: Create a factory which can create the different levels based on a typescript record type
}