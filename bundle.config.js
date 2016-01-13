module.exports = {
    bundle: {
        main: {
            scripts: [
                "./Presentation.Web/app/**/*.js"
            ],
            styles: "./Presentation.Web/Content/**/*.css"
        },
        vendor: {
            scripts: "./Presentation.Web/Scripts/**/*.min.js"
        }
    },
    copy: "./Presentation.Web/Content/**/*.{png,svg}"
};
