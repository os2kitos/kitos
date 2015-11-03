'use strict';
var gulp = require('gulp'),
	tslint = require('gulp-tslint'),
	eslint = require('gulp-eslint'),
	Config = require('./gulpfile.config');
var config = new Config()

gulp.task('default', function() {
  // place code for your default task here
});

gulp.task('ts-lint', function () {
    return gulp.src(config.allTypeScript)
		.pipe(tslint())
		.pipe(tslint.report('prose', {
			summarizeFailureOutput: true
		}));
});

gulp.task('es-lint', function() {
	return gulp.src(config.allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
		// Use this to fail build on errors
		//.pipe(eslint.failAfterError());
});

gulp.task('lint', ['es-lint', 'ts-lint']);
