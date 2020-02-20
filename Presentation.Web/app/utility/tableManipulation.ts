module Kitos.Utility {

    export class TableManipulation {

        static expandRetractParagraphCell(e, shortTextLineCount) {
            var element = angular.element(e.currentTarget);
            var para = element.closest("td").find(document.getElementsByClassName("readMoreParagraph"))[0];
            var btn = element[0];

            if (para.getAttribute("style") != null) {
                para.removeAttribute("style");
                btn.innerText = "Se mindre";
            } else {
                para.setAttribute("style", `height: ${shortTextLineCount}em;overflow: hidden;`);
                btn.innerText = "Se mere";
            }
        }

    }

}