module Kitos.Models.UICustomization {

    /**
     * Defines the valid customization modules for kitos
     */
    export enum CustomizableKitosModule {
        ItSystemUsage = "ItSystemUsages",
        ItContract = "ItContracts"
    }

    export interface IUICustomizationTreeVisitor {
        visitNode(node: UINode);
        visitModule(node: CustomizedModuleUI);
    }

    export interface IUICustomizationTreeMember {
        accept(visitor: IUICustomizationTreeVisitor): void;
    }

    export interface IUICustomizationNodeSearch {
        /**
         * Locates a specific node in the customization tree - must not be used when applying mutations since mutations are subject to hierachial rules
         * @param fullKey
         */
        locateNode(fullKey: string): IUINode;
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
    export interface ICustomizedModuleUI extends IStatefulUICustomizationNode, IUICustomizationTreeMember, IUICustomizationNodeSearch {
        module: CustomizableKitosModule;
        /**
         * Extracts the fullKey from the config object and calls isAvailable
         * @param blueprintObject
         */
        isBluePrintNodeAvailable(blueprintObject: UICustomization.Configs.ICustomizableUINodeConfig): boolean;
        root: IUINode;
    }

    export interface IUINode extends IStatefulUICustomizationNode, IUICustomizationTreeMember, IUICustomizationNodeSearch {
        text: string;
        readOnly: boolean;
        editable: boolean;
        available: boolean;
        key: string;
        children: Array<IUINode>;
        helpText?: string;
    }

    export class UINode implements IUINode {
        private readonly _isReadOnly: boolean;
        private _editable: boolean;
        private _available: boolean;
        private readonly _helpText: string | undefined;
        private readonly _key: string;
        private readonly _children: IUINode[];
        private readonly _childGroupLookup: Record<string, IUINode>;
        private readonly _text: string;

        constructor(key: string, text: string, editable: boolean, available: boolean, isReadOnly: boolean, children: IUINode[], helpText?: string) {
            this._isReadOnly = isReadOnly;
            this._children = children ?? [];
            this._editable = editable && !isReadOnly;
            this._available = available;
            this._key = key;
            this._text = text;
            this._helpText = helpText;
            this._childGroupLookup = {};
            children.forEach(child => {
                const path = this.extractChildPath(child.key);
                if (this._childGroupLookup[path] != undefined) {
                    throw new Error(`Duplicate child path found:${path} for node at level:${key}`);
                }
                this._childGroupLookup[path] = child;
            });
        }

        /**
         * Locate the child that is part of the key path provided in the fullKey. 
         * @param fullKey
         */
        private getChildCallPath(fullKey: string): IUINode {
            const path = this.extractChildPath(fullKey);
            const child = this._childGroupLookup[path];
            if (!child) {
                throw new Error(`Not possible to find a suitable child path based on:${fullKey} in object with key ${this._key}`);
            }
            return child;
        }

        private extractChildPath(key: string): string {
            const matchChildRegex = new RegExp(`^${this._key.replace(".", "\\.")}\.([a-zA-Z]+)(\..*)*$`);
            const results = matchChildRegex.exec(key);
            if (results.length < 2) {
                throw new Error(`${key} does not have a valid key as a child of this level with key: ${this._key}`);
            }
            return results[1];
        }

        locateNode(fullKey: string): IUINode {

            if (fullKey === this._key) {
                return this;
            }

            const child = this.getChildCallPath(fullKey);
            return child.locateNode(fullKey);
        }

        isAvailable(fullKey: string): boolean {
            if (this._available) {
                if (fullKey === this._key) {
                    return true;
                }

                const child = this.getChildCallPath(fullKey);
                return child.isAvailable(fullKey);
            }

            //If disabled on this level, then no child can be enabled
            return false;
        }

        changeAvailableState(fullKey: string, newState: boolean): void {
            if (fullKey === this.key) {
                if (!this.editable) {
                    throw new Error(`Cannot change ui customization node ${this._key} since it is not editable.`);
                }
                if (newState !== this._available) {
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
                const child = this.getChildCallPath(fullKey);
                child.changeAvailableState(fullKey, newState);
            } else {
                throw new Error(`Cannot change state of descendant ${fullKey} if ancestor with key ${this.key} is unavailable`);
            }
        }

        accept(visitor: IUICustomizationTreeVisitor): void {
            visitor.visitNode(this);
            this.children.forEach(child => child.accept(visitor));
        }

        get children(): IUINode[] { return this._children; }

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

        get text() { return this._text; }

        get helpText() { return this._helpText; }
    }


    export class CustomizedModuleUI implements ICustomizedModuleUI {
        private readonly _root: IUINode;
        private readonly _module: CustomizableKitosModule;

        constructor(module: CustomizableKitosModule, root: IUINode) {
            this._root = root;
            this._module = module;
        }

        locateNode(fullKey: string): IUINode {
            return this._root.locateNode(fullKey);
        }

        isBluePrintNodeAvailable(blueprintObject: UICustomization.Configs.ICustomizableUINodeConfig): boolean {
            return this.isAvailable(blueprintObject.fullKey);
        }

        isAvailable(fullKey: string): boolean {
            return this._root.isAvailable(fullKey);
        }

        changeAvailableState(fullKey: string, newState: boolean): void {
            this._root.changeAvailableState(fullKey, newState);
        }

        get module() { return this._module; }

        get root() { return this._root; }

        accept(visitor: IUICustomizationTreeVisitor): void {
            visitor.visitModule(this);
            this._root.accept(visitor);
        }
    }
}