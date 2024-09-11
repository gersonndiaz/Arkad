window.fbControls.push(function (controlClass) {
    class controlCameraButton extends controlClass {
        static get definition() {
            return {
                icon: '📷',
                i18n: {
                    default: 'Capturar Imagen',
                },
                label: 'camera'
            };
        }

        build() {
            this.dom = this.markup('div', [
                this.markup('button', 'Capturar Imagen', {
                    className: 'btn btn-primary',
                    onclick: this.captureImage.bind(this)
                })
            ]);
            return this.dom;
        }

        captureImage() {
            // Implementar lógica para capturar imagen
        }
    }

    controlClass.register('camera-image', controlCameraButton);
    return controlCameraButton;
});
