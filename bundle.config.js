module.exports = {
    bundle: {
        main: {
            scripts: [
                './Presentation.Web/Scripts/app/**/*.js'
            ]
        },
        vendor: {

        }
    },
    copy: './Presentation.Web/Content/**.*{png,svg}'
};
