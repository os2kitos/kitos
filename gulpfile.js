'use strict';
var gulp = require('gulp'),
    tslint = require('gulp-tslint'),
    eslint = require('gulp-eslint'),
    bundle = require('gulp-bundle-assets');

// Files selections
var source = 'Presentation.Web/Scripts/',
    sourceApp = source + 'app',
    allJavaScript = sourceApp + '/**/*.js',
    allTypeScript = sourceApp + '/**/*.ts';

gulp.task('default', ['lint']);

gulp.task('lint', ['es-lint', 'ts-lint']);

gulp.task('ts-lint', function () {
    return gulp.src(allTypeScript)
		.pipe(tslint())
		.pipe(tslint.report('prose', {
            emitError: false // Set to true to fail build on errors
		}));
});

gulp.task('es-lint', function() {
	return gulp.src(allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
		// Use this to fail build on errors
		//.pipe(eslint.failAfterError());
});

gulp.task('bundle', function () {
    return gulp.src('./bundle.config.js')
      .pipe(bundle())
      .pipe(bundle.results('./public'))
      .pipe(gulp.dest('./public'));
});

gulp.task('watch', function () {
    gulp.watch(allTypeScript, ['ts-lint']);
    gulp.watch(allJavaScript, ['es-lint']);
});
