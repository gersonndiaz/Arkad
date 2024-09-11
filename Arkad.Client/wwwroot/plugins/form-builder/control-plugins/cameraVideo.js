window.fbControls.push(function (controlClass) {
    class controlCameraButton extends controlClass {
        static get definition() {
            return {
                icon: '📷',
                i18n: {
                    default: 'Capturar Video',
                },
                label: 'camera'
            };
        }

        build() {
            this.dom = this.markup('div', [
                this.markup('button', 'Capturar Video', {
                    className: 'btn btn-primary',
                    onclick: this.captureVideo.bind(this)
                })
            ]);
            return this.dom;
        }

        captureVideo() {
            // Implementar lógica para capturar video
        }
    }

    controlClass.register('camera-video', controlCameraButton);
    return controlCameraButton;
});
