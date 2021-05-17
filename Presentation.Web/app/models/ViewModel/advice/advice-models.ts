
module Kitos.Models.ViewModel.Advice {
    import UpdatedSelect2OptionViewModel = ViewModel.Generic.UpdatedSelect2OptionViewModel;

    export enum AdviceType {
        Immediate = "0",
        Repeat = "1"
    }

    export enum AdviceRepetition {
        Immediate = "0", //Exists in DB due to backwards compatibility
        Hour = "1",
        Day = "2",
        Week = "3",
        Month = "4",
        Year = "5",
        Quarter = "6",
        Semiannual = "7"
    }

    export class AdviceTypeOptions {
        static readonly options = [
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceType.Immediate, text: "Straks" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceType.Repeat, text: "Gentagelse" }
        ];

        static getOptionFromEnumString(input: string) {

            switch (AdviceType[input]) {
                case AdviceType.Immediate:
                    return _.find(this.options, x => x.id === AdviceType.Immediate);
                case AdviceType.Repeat:
                    return _.find(this.options, x => x.id === AdviceType.Repeat);
                default:
                    return null;
            }
        }
    }

    export class AdviceRepetitionOptions {
        static readonly options = [
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Hour, text: "Time" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Day, text: "Dag" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Week, text: "Uge" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Month, text: "Måned" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Quarter, text: "Kvartal" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Semiannual, text: "Halvårlig" },
            <UpdatedSelect2OptionViewModel<any>>{ id: AdviceRepetition.Year, text: "År" }
        ];

        static getOptionFromEnumString(input: string) {
            switch (AdviceRepetition[input]) {
                case AdviceRepetition.Immediate:
                    return _.find(this.options, x => x.id === AdviceRepetition.Immediate);
                case AdviceRepetition.Day:
                    return _.find(this.options, x => x.id === AdviceRepetition.Day);
                case AdviceRepetition.Hour:
                    return _.find(this.options, x => x.id === AdviceRepetition.Hour);
                case AdviceRepetition.Week:
                    return _.find(this.options, x => x.id === AdviceRepetition.Week);
                case AdviceRepetition.Month:
                    return _.find(this.options, x => x.id === AdviceRepetition.Month);
                case AdviceRepetition.Quarter:
                    return _.find(this.options, x => x.id === AdviceRepetition.Quarter);
                case AdviceRepetition.Semiannual:
                    return _.find(this.options, x => x.id === AdviceRepetition.Semiannual);
                case AdviceRepetition.Year:
                    return _.find(this.options, x => x.id === AdviceRepetition.Year);

                default:
                    return null;
            }
        }
    }



}

