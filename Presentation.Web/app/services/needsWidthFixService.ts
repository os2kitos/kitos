module Kitos.Services {
    export class NeedsWidthFix {

        // Fixes the blank spaces problem when deselecting columns (OS2KITOS-607)

        static $inject = ["$"];

        constructor(private $: JQueryStatic) { }

        public fixWidthOnClick = () => {
            this.$("body").on("click", ".k-item .k-state-default .k-link", () => {
                this.fixWidth();
            });
        }

        public fixWidth = () => {
            this.$(".k-grid-content table").css("width", "100%");
            this.$(".k-grid-header-wrap table").css("width", "100%");
        }
    }

    app.service("needsWidthFixService", NeedsWidthFix);
}