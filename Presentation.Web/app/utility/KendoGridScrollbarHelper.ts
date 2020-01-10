module Kitos.Utility.KendoGrid {
    export class KendoGridScrollbarHelper {
        static resetScrollbarPosition(grid: IKendoGrid<any>) {
            grid.content.scrollTop(0);
            grid.content.scrollLeft(0);
        }
    }
}