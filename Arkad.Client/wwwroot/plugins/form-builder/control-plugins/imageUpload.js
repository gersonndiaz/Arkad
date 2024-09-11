
/**
 * Uso de imagenes
 */

// configure the class for runtime loading
if (!window.fbControls) window.fbControls = []
window.fbControls.push(function (controlClass) {

    class controlImageUpload extends controlClass {
        static get definition() {
            return {
                icon: '📷',
                label: 'Image Upload',
                i18n: {
                    default: 'Image Upload',
                },
            };
        }

        configure() {
            // Carga las dependencias necesarias, como jQuery u otras bibliotecas externas
            this.js = '../../jquery/jquery-3.6.3.min.js';
        }

        build() {
            // Crea la estructura HTML para el campo de carga de imágenes
            return this.markup('input', null, {
                id: this.config.name,
                type: 'file',
                accept: 'image/*',
            });
        }

        onRender() {
            // Asigna un evento al cambio del campo de carga de imágenes
            $('#' + this.config.name).on('change', (event) => {
                const file = event.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = (e) => {
                        const imagePreview = this.markup('img', null, {
                            id: 'imagePreview',
                            src: e.target.result,
                        });
                        // Agrega la imagen previa al formulario
                        this.input.appendChild(imagePreview);
                    };
                    reader.readAsDataURL(file);
                }
            });
        }
    }

    controlClass.register('imageUpload', controlImageUpload);

});