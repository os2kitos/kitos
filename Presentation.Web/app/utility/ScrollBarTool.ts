module Kitos.Utility {
    export class ScrollBarHelper {
        static resetScrollbarPosition(grid: IKendoGrid<any>) {
            grid.content.scrollTop(0);
            grid.content.scrollLeft(0);
        }
    }
}