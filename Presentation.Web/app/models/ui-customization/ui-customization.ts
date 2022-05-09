module Kitos.Models.UICustomization {

    /**
     * Defines the valid customization modules for kitos
     */
    export enum CustomizableKitosModule {
        ItSystemUsage = "it-system-usage"
    }

    export interface IStatefulUICustomizationNode {
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

    /**
     * Defines a customized KITOS module including "module.group[1..*].setting
     */
    export interface ICustomizedModuleUI extends IStatefulUICustomizationNode {
        module: CustomizableKitosModule;
    }

    export interface IUINode extends IStatefulUICustomizationNode {
        readOnly: boolean;
        editable: boolean;
        available: boolean;
        key: string;
        children: Array<IUINode>;
    }

    class UINode implements IUINode {
        private readonly _isReadOnly: boolean;
        private _editable: boolean;
        private _available: boolean;
        private readonly _key: string;
        private readonly _children: IUINode[];
        private readonly _childGroupLookup: Record<string, IUINode>;

        constructor(editable: boolean, available: boolean, key: string, children: IUINode[], isReadOnly: boolean) {
            this._isReadOnly = isReadOnly;
            this._children = children;
            this._editable = editable && !isReadOnly;
            this._available = available;
            this._key = key;
            this._childGroupLookup = {};
            children.forEach(child => {
                const path = this.extractChildPath(child.key);
                if (this._childGroupLookup[path] != undefined) {
                    throw new Error(`Duplicate child path found:${path} for node at level:${key}`);
                }
                this._childGroupLookup[path] = child;
            });
        }

        private extractChildPath(key: string): string {
            const matchChildRegex = new RegExp(`^${this._key.replace(".","\\")}\.([a-zA-Z\-]+)(\..*)*$`);
            const results = matchChildRegex.exec(key);
            if (results.length < 2) {
                throw new Error(`${key} does not have a valid key as a child of this level with key: ${this._key}`);
            }
            return results[1];
        }

        isAvailable(fullKey: string): boolean {
            if (this._available) {
                if (fullKey === this._key) {
                    return true;
                }

                const path = this.extractChildPath(fullKey);
                const child = this._childGroupLookup[path];
                if (!child) {
                    console.warn(path, " belongs to this ui customization tree, but is not defined in the persisted structure. This indicates that it is a new customization field which has not been configured by local admin yet, so by default it is available.");
                    return true;
                }
                return child.isAvailable(fullKey);
            }

            //If disabled on this level, then no child can be enabled
            return false;
        }

        changeAvailableState(fullKey: string, newState: boolean): void {
            if (fullKey === this.key) {
                if (this.editable) {
                    throw new Error(`Cannot change ui customization node ${this._key} since it is not editable.`);
                } else if (newState !== this._available) {
                    this._available = newState;
                    this.children
                        .filter(child => child.readOnly === false)
                        .forEach(child => {
                            if (newState) {
                                child.editable = true; //enable editing before changing state
                            }
                            child.changeAvailableState(child.key, newState);
                            if (!newState) {
                                child.editable = false; //enable editing of children while parent is disabled
                            }
                        });
                }
            } else if (this.available) {
                const path = this.extractChildPath(fullKey);
                const child = this._childGroupLookup[path];
                if (!child) {
                    throw new Error(`No available path to descendant with key ${fullKey} could be found on ui node ${this._key}`);
                }
                child.changeAvailableState(fullKey, newState);
            } else {
                throw new Error(`Cannot change state of descendant ${fullKey} if ancestor with key ${this.key} is unavailable`);
            }
        }

        get children(): IUINode[] { return [...this._children]; }

        get readOnly() { return this._isReadOnly; }

        get editable() { return this._isReadOnly === false && this._editable; }
        set editable(newState: boolean) {
            if (this._isReadOnly) {
                throw new Error(`Cannot change editable state of ${this._key} since it is read-only`);
            }

            if (this._editable !== newState) {
                this._editable = newState;
                this.children
                    .filter(child => child.readOnly === false)
                    .forEach(child => child.editable = newState);
            }
        }

        get available() { return this._available; }

        get key() { return this._key; }
    }


    class CustomizedModuleUI implements ICustomizedModuleUI {
        private readonly _root: IUINode;
        private readonly _module: CustomizableKitosModule;

        constructor(module: CustomizableKitosModule, root: IUINode) {
            this._root = root;
            this._module = module;
        }

        isAvailable(fullKey: string): boolean {
            return this._root.isAvailable(fullKey);
        }

        changeAvailableState(fullKey: string, newState: boolean): void {
            this._root.changeAvailableState(fullKey, newState);
        }


        get module() { return this._module; }
    }

    //TODO: Merge server config into structure?

    //TODO: Create a factory which can create the different levels based on a typescript record type

    /*
     *TODO: A service to get the active configuration:
     * - Get the blueprimt
     * - Get the server config and create a dictionary with key->state
     * - Based on the blueprint, recursively build the tree and use the server config to override known keys (depth first to prevent issues caused by the hierarchy).
     * - for each key used, add a "used" boolean to the state
     * - If any keys are not "used", add a warning to the console to simplify checks...
     */
}