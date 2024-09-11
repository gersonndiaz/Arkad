/**
 * Área de dibujo
 */

// configure the class for runtime loading
if (!window.fbControls) window.fbControls = []
window.fbControls.push(function (controlClass) {
    /**
     * Star rating class
     */
    class controlDrawing extends controlClass {
        /**
         * Class configuration - return the icons & label related to this control
         * @returndefinition object
         */
        static get definition() {
            return {
                icon: '✍',
                label: "drawing",
                type: "canvas",
                i18n: {
                    default: 'Área Dibujo',
                },
            }
        }

        /**
     * javascript & css to load
     */
        configure() {
            this.js = '/js/html2canvas.min.js'
        }

        /**
         * build a text DOM element, supporting other jquery text form-control's
         * @return {Object} DOM Element to be injected into the form.
         */
        build() {

            const css = this.markup(
                'style',
                `
                  canvas {
                    border: 1px solid;
                  }`,
                { type: 'text/css' },
            )

            return this.markup('canvas', [css], { id: this.config.name })
        }

        /**
         * onRender callback
         */
        onRender() {
            const canvas = document.getElementById(this.id);
            var ctx = canvas.getContext("2d");
            var cw = canvas.width = 300,
                cx = cw / 2;
            var ch = canvas.height = 300,
                cy = ch / 2;

            var dibujar = false;
            ctx.lineJoin = "round";

            canvas.addEventListener('mousedown', function (evt) {
                dibujar = true;
                //ctx.clearRect(0, 0, cw, ch);
                ctx.beginPath();

            }, false);

            canvas.addEventListener('mouseup', function (evt) {
                dibujar = false;

            }, false);

            canvas.addEventListener("mouseout", function (evt) {
                dibujar = false;
            }, false);

            canvas.addEventListener("mousemove", function (evt) {
                if (dibujar) {
                    var m = oMousePos(canvas, evt);
                    ctx.lineTo(m.x, m.y);
                    ctx.stroke();
                }
            }, false);

            function oMousePos(canvas, evt) {
                var ClientRect = canvas.getBoundingClientRect();
                return { //objeto
                    x: Math.round(evt.clientX - ClientRect.left),
                    y: Math.round(evt.clientY - ClientRect.top)
                }
            }
        }
    }

    function captureToImage(idElement) {
        html2canvas(document.querySelector('#' + idElement)).then(canvas => {
            //document.body.appendChild(canvas)

            //var mywindow = window.open('', 'PRINT', 'height=600,width=800');
            var mywindow = window.open();

            mywindow.document.write('<html><head><title>' + document.title + '</title>');
            mywindow.document.write('</head><body >');
            mywindow.document.write('<img src="' + canvas.toDataURL() + '"/>');
            mywindow.document.write('<style>table, th, td {text-align: left;} tr {border-bottom: 1px solid silver;} #buscador{display: none;} .hide{display:none;} .tableexport-ignore{display:none;}</style>');
            mywindow.document.write('</body></html>');

            mywindow.document.close(); // necessary for IE >= 10
            mywindow.focus(); // necessary for IE >= 10*/

            mywindow.print();
        });
    }

    function clearCanvas(id) {
        var canvas = document.getElementById(id);
        var context = canvas.getContext('2d');
        context.clearRect(0, 0, canvas.width, canvas.height); //clear html5 canvas
    }

    // register this control for the following types & text subtypes
    controlClass.register('canvas', controlDrawing)
    return controlDrawing
})
