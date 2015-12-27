var gulp = require('gulp'),
    paths = require('../paths.config.js');

gulp.task('lint', ['es-lint', 'ts-lint']);

// run tslint on all typescript files.
gulp.task('ts-lint', function () {
    var tslint = require('gulp-tslint');

    //return gulp.src(paths.allTypeScript)
    return gulp.src(['Presentation.Web/Tests/**/*.ts'])
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
