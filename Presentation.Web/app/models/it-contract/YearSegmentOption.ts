module Kitos.Models.ItContract {
    import UpdatedSelect2OptionViewModel = Models.ViewModel.Generic.UpdatedSelect2OptionViewModel;

    export enum YearSegmentOption {
        EndOfCalendarYear = "0",
        EndOfQuarter = "1",
        EndOfMonth = "2"
    }

    export class YearSegmentOptions {
        options: UpdatedSelect2OptionViewModel<string>[];
        static readonly options = [
            <UpdatedSelect2OptionViewModel<string>>{ id: YearSegmentOption.EndOfCalendarYear, optionalObjectContext: "EndOfCalendarYear", text: "Kalenderår" },
            <UpdatedSelect2OptionViewModel<string>>{ id: YearSegmentOption.EndOfQuarter, optionalObjectContext: "EndOfQuarter", text: "Kvartal" },
            <UpdatedSelect2OptionViewModel<string>>{ id: YearSegmentOption.EndOfMonth, optionalObjectContext: "EndOfMonth", text: "Måned" }
        ];

        static getYearSegmentOptionContextToTextMap() {
            return _.reduce(this.options,
                (acc: any, current) => {
                    acc[current.optionalObjectContext] = current.text;
                    return acc;
                },
                <any>{});
        }

        static getFromOption(yearSegmentOption: number | string) {
            switch (yearSegmentOption) {
                case null:
                    return null;
                case 0:
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfCalendarYear)[0];
                case 1:
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfQuarter)[0];
                case 2:
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfMonth)[0];
                case "0":
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfCalendarYear)[0];
                case "1":
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfQuarter)[0];
                case "2":
                    return this.options.filter(x => x.id === YearSegmentOption.EndOfMonth)[0];

                default:
                    throw new RangeError(`${yearSegmentOption} is not a valid yearSegment Option`);
            }
        }
    }
}