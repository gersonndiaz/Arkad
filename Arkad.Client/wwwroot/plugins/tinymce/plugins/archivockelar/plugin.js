
/*
  Note: We have included the plugin in the same JavaScript file as the TinyMCE
  instance for display purposes only. Tiny recommends not maintaining the plugin
  with the TinyMCE instance and using the `external_plugins` option.
*/
tinymce.PluginManager.add('archivockelar', function (editor, url) {
    var openDialog = function () {
        return editor.windowManager.open({
            title: 'Cargar Archivo',
            body: {
                type: 'panel',
                items: [
                    {
                        type: 'urlinput',
                        filetype: "file",
                        subtype: "file",
                        name: 'file',
                        label: 'Documento'
                    }
                ]
            },
            buttons: [
                {
                    type: 'cancel',
                    text: 'Close'
                },
                {
                    type: 'submit',
                    text: 'Save',
                    primary: true
                }
            ],
            onSubmit: function (api) {
                try {
                    var data = api.getData();

                    //if (data.file.meta.title.endsWith('.pdf')) {
                    //    console.log(data);

                    //    editor.insertContent(data.file.value);
                    //    //editor.setContent(data.file.value, { format: 'raw' });
                    //}
                    //else {
                    //    alert('El documento no es un archivo PDF!.');
                    //}
                    editor.insertContent(data.file.value);
                }
                catch (err) {
                    console.error(err);
                    alert('No se pudo cargar el archivo. Intentelo nuevamente.');
                }


                api.close();
            }
        });
    };
    /* Add a button that opens a window */
    editor.ui.registry.addButton('archivockelar', {
        text: 'Archivo',
        icon: 'upload',
        onAction: function () {
            /* Open window */
            openDialog();
        }
    });
    /* Adds a menu item, which can then be included in any menu via the menu/menubar configuration */
    editor.ui.registry.addMenuItem('archivockelar', {
        text: 'archivockelar plugin',
        onAction: function () {
            /* Open window */
            openDialog();
        }
    });
    /* Return the metadata for the help plugin */
    return {
        getMetadata: function () {
            return {
                name: 'archivockelar plugin',
                //url: 'http://archivockelarplugindocsurl.com'
            };
        }
    };
});

/*
  The following is an archivockelar of how to use the new plugin and the new
  toolbar button.
*/
tinymce.init({
    selector: 'textarea#custom-plugin',
    plugins: 'archivockelar help',
    toolbar: 'archivockelar | help'
});

