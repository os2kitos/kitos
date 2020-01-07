"use strict";

var {src, series} = require("gulp");
var log = require("fancy-log");
var protractor = require("gulp-protractor");
var del = require("del");
var paths = require("../paths.config.js");


const cleanProtractor = function(cb) {
    return del("tmp");
}


const protractorHeadless = function (done) {
    const params = process.argv;
    const args = params.length === 6 ? [params[3], params[4], params[5]] : [];

    src(paths.e2eFiles) 
        .pipe(protractor.protractor({
            configFile: "protractor.headless.conf.js",
            args: [
                "--params.login.email", args[0],
                "--params.login.pwd", args[1],
                "--baseUrl", args[2]
            ],
            "debug": false
        }))
        .on("error", function (err) {
            log.error(err);
            throw err;
        })
        .on("end", function () {
            done();
        });
}


const protractorLocal = function(done) {
    const params = process.argv;
    const args = params.length === 6 ? [params[3], params[4], params[5]] : [];

    log.info(`e2e arguments: ${args}`);
    
    src(paths.e2eFiles) 
        .pipe(protractor.protractor({
            configFile: "protractor.conf.js",
            args: [
                "--params.login.email", args[0],
                "--params.login.pwd", args[1],
                "--baseUrl", args[2]
            ],
            "debug": false
        }))
        .on("error", function (err) {
            log.error(err);
            throw err;
        })
        .on("end", function () {
            done();
        });
}

const protractorSingle = function (done) {
    const params = process.argv;
    const args = params.length === 7 ? [params[3], params[4], params[5], params[6]] : [];

    log.info(`e2e arguments: ${args}`);

    const singleSpec = args[3].split("=")[1];
    const singleSpecPath = `${paths.source}/Tests/${singleSpec}`;
    log.info(singleSpecPath);
    src(singleSpecPath) 
        .pipe(protractor.protractor({
            configFile: "protractor.conf.js",
            args: [
                "--params.login.email", args[0],
                "--params.login.pwd", args[1],
                "--baseUrl", args[2]
            ],
            "debug": false
        }))
        .on("error", function (err) {
            log.error(err);
            throw err;
        })
        .on("end", function () {
            done();
        });
}

exports.runProtractorHeadless = series(cleanProtractor, protractorHeadless);
exports.runProtractorLocal = series(cleanProtractor, protractorLocal);
exports.runProtractorSingle = series(cleanProtractor, protractorSingle);
