var gulp = require('gulp'),
    paths = require('../paths.config.js');

gulp.task('lint', ['es-lint', 'ts-lint']);

// run tslint on all typescript files.
gulp.task('ts-lint', function () {
    var tslint = require('gulp-tslint');

    var args = process.argv;
    var path = paths.allTypeScript;
    if (args && args.length > 3 && args[3] === '--path') {
        path = args[4];
    }

    return gulp.src(path)
		.pipe(tslint())
		.pipe(tslint.report('prose', {
		    emitError: false // Set to true to fail build on errors
		}));
});

// run eslint on all javascript files
gulp.task('es-lint', function () {
    var eslint = require('gulp-eslint');

    return gulp.src(paths.allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
    // Use this to fail build on errors
    //.pipe(eslint.failAfterError());
});
