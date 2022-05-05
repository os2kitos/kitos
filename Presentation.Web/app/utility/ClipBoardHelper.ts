module Kitos.Utility {
    export function copyPageContentToClipBoard(contentRootId: string) {
        window.getSelection().selectAllChildren(document.getElementById(contentRootId));
        document.execCommand("Copy");
        window.getSelection().removeAllRanges();
    }
}