module Kitos.Helpers {

    export class OptionEntityHelper {

        static createDictionaryFromOptionList(options: Kitos.Models.IOptionEntity[]) {
            return options.reduce((acc, next, _) => {
                acc[next.Id] = {
                    text: next.Name,
                    id: next.Id,
                    optionalObjectContext: {
                        id: next.Id,
                        name: next.Name,
                        description: next.Description
                    }
                };
                return acc;
            }, {});
        };
    }
}