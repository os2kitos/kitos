var gulp = require("gulp"),
    paths = require("../paths.config.js");

// bundle files for deployment.
gulp.task("bundle", function () {
    var bundle = require("gulp-bundle-assets");

    return gulp.src("./bundle.config.js")
      .pipe(bundle())
      .pipe(bundle.results(paths.bundleDir))
      .pipe(gulp.dest((paths.bundleDir)));
});
