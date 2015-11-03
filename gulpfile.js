'use strict';
var gulp = require('gulp'),
	tslint = require('gulp-tslint'),
	eslint = require('gulp-eslint'),
    bundle = require('gulp-bundle-assets'),
	Config = require('./gulpfile.config');
var config = new Config()

gulp.task('default', function() {
  // place code for your default task here
});

gulp.task('lint', ['es-lint', 'ts-lint']);

gulp.task('ts-lint', function () {
    return gulp.src(config.allTypeScript)
		.pipe(tslint())
		.pipe(tslint.report('prose', {
            emitError: false // Set to true to fail build on errors
		}));
});

gulp.task('es-lint', function() {
	return gulp.src(config.allJavaScript)
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
