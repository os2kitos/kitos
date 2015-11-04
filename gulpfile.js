var gulp = require('gulp'),
    tslint = require('gulp-tslint'),
    eslint = require('gulp-eslint'),
    bundle = require('gulp-bundle-assets'),
    paths = require('./paths.config.js'),
    KarmaServer = require('karma').Server;

gulp.task('default', ['lint']);

gulp.task('lint', ['es-lint', 'ts-lint']);

gulp.task('ts-lint', function () {
    return gulp.src(paths.allTypeScript)
		.pipe(tslint())
		.pipe(tslint.report('prose', {
            emitError: false // Set to true to fail build on errors
		}));
});

gulp.task('es-lint', function() {
    return gulp.src(paths.allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
		// Use this to fail build on errors
		//.pipe(eslint.failAfterError());
});

gulp.task('bundle', function () {
    return gulp.src('./bundle.config.js')
      .pipe(bundle())
      .pipe(bundle.results(paths.bundleDir))
      .pipe(gulp.dest((paths.bundleDir)));
});

gulp.task('watch', function () {
    gulp.watch(paths.allTypeScript, ['ts-lint']);
    gulp.watch(paths.allJavaScript, ['es-lint']);
});

gulp.task('karma', function () {
    new KarmaServer({
        configFile: __dirname + '/' + paths.source + '/karma.conf.js',
        singleRun: true,
        reporters: ['progress', 'coverage', 'coveralls'],
        autoWatch: false
    }).start();
});
