module Kitos {
    // Helper factory to help angular (v1) instantiate directives made with typescript classes
    export class DirectiveFactory {
        public static getFactoryFor(classType): ng.IDirectiveFactory {
            const factory = (...args: any[]): ng.IDirective => {
                var newInstance = Object.create(classType.prototype);
                newInstance.constructor.apply(newInstance, args);
                return newInstance;
            };
            factory.$inject = classType.$inject;
            return factory;
        }
    }
}
