module Kitos.Utility {

    export class TableManipulation {

        static expandRetractParagraphCell(e, shortTextLineCount) {
            const element = angular.element(e.currentTarget);
            const para = element.closest("td").find(document.getElementsByClassName("readMoreParagraph"))[0];
            const btn = element[0];

            if (para.getAttribute("data-textExpanded") != null) {
                para.removeAttribute("data-textExpanded");
                para.setAttribute("style", `height: ${shortTextLineCount}em;overflow: hidden;`);
                btn.innerText = "Se mere";
            } else {
                para.setAttribute("data-textExpanded", "");
                para.removeAttribute("style");
                btn.innerText = "Se mindre";
            }
        }

    }

}