var paths = require("./paths.config.js");

module.exports = {
    // font bundle
    fontSrc: [
        bower("/bootstrap/dist/fonts/*.*"),
        bower("/font-awesome/fonts/*.*")
    ],

    // assets
    assetsSrc: [
        bower("/select2/*.png"),
        bower("/select2/*.gif")
    ],

    fontDest: content("/fonts"),
    cssDest: content("/css"),
    maps: "maps",

    content: content,
    bower: bower
};

// path helper functions
function script(file) {
    return paths.sourceScript + "/" + file;
}

function content(file) {
    return paths.source + "/Content" + file;
}

function bower(file) {
    return paths.bowerComponents + "/" + file;
}
