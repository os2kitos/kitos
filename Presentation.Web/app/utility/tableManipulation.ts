module Kitos.Utility {

    export class TableManipulation {
        

        static expandRetractParagraphCell(e, shortTextLineCount) {
            const textExpandedAttribute = "data-textExpanded";
            const element = angular.element(e.currentTarget);
            const para = element.closest("td").find(document.getElementsByClassName("readMoreParagraph"))[0];
            const btn = element[0];

            if (para.getAttribute(textExpandedAttribute) != null) {
                para.removeAttribute(textExpandedAttribute);
                para.setAttribute("style", `height: ${shortTextLineCount}em;overflow: hidden;`);
                btn.innerText = "Se mere";
            } else {
                para.setAttribute(textExpandedAttribute, "");
                para.removeAttribute("style");
                btn.innerText = "Se mindre";
            }
        }

    }

}