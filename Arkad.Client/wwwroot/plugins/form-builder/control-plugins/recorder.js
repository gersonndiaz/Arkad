window.fbControls.push(function (controlClass) {
    class controlAudioRecorder extends controlClass {
        static get definition() {
            return {
                icon: '🎤',
                i18n: {
                    default: 'Grabar Audio',
                },
                label: 'audio'
            };
        }

        build() {
            this.dom = this.markup('button', 'Grabar Audio', {
                className: 'btn btn-primary recorder',
                onclick: this.startRecording.bind(this)
            });
            return this.dom;
        }

        startRecording() {
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                console.error('El dispositivo no soporta grabación de audio');
                return;
            }
            navigator.mediaDevices.getUserMedia({ audio: true })
                .then(stream => {
                    const mediaRecorder = new MediaRecorder(stream);
                    mediaRecorder.start();

                    const audioChunks = [];
                    mediaRecorder.addEventListener("dataavailable", event => {
                        audioChunks.push(event.data);
                    });

                    mediaRecorder.addEventListener("stop", () => {
                        const audioBlob = new Blob(audioChunks);
                        const audioUrl = URL.createObjectURL(audioBlob);
                        const audio = new Audio(audioUrl);
                        audio.play();
                    });

                    setTimeout(() => {
                        mediaRecorder.stop();
                    }, 60000); // Graba durante 1 minuto, ajusta según necesidad
                })
                .catch(error => {
                    console.error('Error al acceder al micrófono:', error);
                });
        }
    }

    controlClass.register('audio', controlAudioRecorder);
    return controlAudioRecorder;
});
