# PikchrViewer
Visual Studio extension which renders Pikchr files with the extension `.pikchr.svg`

I'm using this in combination with a Gulp task to convert the Pikchr markup to SVG format, e.g.

```
/**
 * Converts Pikchr diagrams (text files containing Pikchr markup with the file extension .pikchr)
 * into SVG files with the file extension .pikchr.svg.  If the VS Extension 'PikchrViewer' is
 * installed, when these SVG files are opened in VS, they will appear as images.  The VS extension
 * can be downloaded from here:
 * https://marketplace.visualstudio.com/items?itemName=edwardsmale.PikchrViewer
 */
gulp.task("diagrams-build", function (done) {

    const pikchr = require("pikchr");
    const through = require("through2");
    const rename = require("gulp-rename");

    gulp.src("./**/*.pikchr")
        .pipe(through.obj(function (file, _, callback) {
            if (file.isBuffer()) {
                const newContents = pikchr.pikchr(file.contents.toString());
                file.contents = Buffer.from(newContents);
            }
            callback(null, file);
        }))
        .pipe(rename(function (path) {
            path.extname = ".pikchr.svg";
        }))
        .pipe(gulp.dest("."))
        .on("end", done);
});
```

Hacking together the Gulp task was a task I hope not to have to repeat often... what a nightmare.
