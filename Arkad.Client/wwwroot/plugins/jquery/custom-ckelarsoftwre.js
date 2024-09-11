//Copyright Ckelar Software SpA https://www.ckelar.cl/


//#region Obtener IP Cliente
function getClientIp(idElement) {
    $.getJSON("https://api.ipify.org?format=json",
        function (data) {
            //alert('IP: ' + data.ip);
            //clientIp = data.ip;
            //return data.ip;

            $("#" + idElement).val(data.ip);
        });
}
//#endregion Obtener IP Cliente

//#region Enviar por Ajax
function sendData(element, event, action, method, isForm, loadingMessage) {

    event.preventDefault();

    var formData = null;
    if (isForm) {
        formData = new FormData(element);
        //console.log(method);
        //console.log(url);
        //for (var pair of formData.entries()) {
        //    console.log(pair[0] + ', ' + pair[1]);
        //}

        if (action == "" || action == null) {
            action = $(element).attr('action');
        }
        if (method == "" || method == null) {
            method = $(element).attr('method');
        }

    }

    $.ajax({
        url: action,
        type: method,
        data: formData,
        processData: false,
        contentType: false,
        dataType: "html",
        beforeSend: function () {
            if (loadingMessage !== "" && loadingMessage != null) {
                $('#loadingModalLabel').html(loadingMessage);
            }
            $('#loadingModal').modal('show');
        },
        success: function (result) {
            processJsonResponse(result);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log('Status: ' + textStatus);
            console.log('Error: ' + errorThrown);
            console.log('jqXHR: ' + jqXHR);

            if (jqXHR.responseText.startsWith('<!doctype html>')){
                alert(errorThrown);
                console.error(errorThrown);
            }
            else if (jqXHR && jqXHR.responseText) {
                processJsonResponse(jqXHR.responseText);
            }
            else {
                alert(errorThrown);
                console.error(errorThrown);
            }
            //setTimeout(function () {
            //    $('#loadingModal').modal('hide');
            //}, 500);
        },
        complete: function (xhr, status) {
            console.log('xhr: ' + xhr);
            //setTimeout(function () {
            //    $('#loadingModal').modal('hide');
            //}, 500);
        }
    });

}


function processJsonResponse(result) {
    try {
        console.log(result);
        var res = JSON.parse(result);
        //var res = JSON.parse(obj);
        console.log(res);

        if (res.urlRedirect) {
            console.log('URL Redirect: ' + res.urlRedirect);
            //window.location.href = res.urlRedirect;
            showAlertMessage(res.subject, res.message);
            setTimeout(function () {
                window.location.href = res.urlRedirect;
            }, 2000);
        }
        else if (res.reload) {
            console.log('Reload: ' + res.reload);
            //alert(res.message);
            //window.location.reload(true);
            showAlertMessage(res.subject, res.message);
            setTimeout(function () {
                window.location.reload(true);
            }, 2000);
        }
        else {
            //alert(res.message);
            showAlertMessage(res.subject, res.message);
        }
    } catch (err) {
        //alert(err);
        //console.error(err);
        showAlertMessage('Error', err);
    }
}
//#endregion Enviar por Ajax

//#region Mostrar y Ocultar Inputs Password
function showAndHideInputPassword(element) {
    var x = document.getElementById(element.id);
    if (x.type === "password") {
        x.type = "text";
    } else {
        x.type = "password";
    }
}
//#endregion Mostrar y Ocultar Inputs Password

function showAlertMessage(title, message)
{
    $('#alertMessageModalLabel').html(title);
    $('#alertMessageModalBody').html(message);

    closeAllModal();

    $('#alertMessageModal').modal('show');
}

function closeAllModal() {
    //$('.modal-backdrop').each(function () {
    //    $(this).remove();
    //});
    //$('.modal').each(function () {
    //    $(this).modal('hide');
    //});
    //$('body').removeClass('modal-open');

    // Ocultar todos los modales abiertos
    $('.modal').modal('hide');

    // Eliminar todos los backdrop que queden en el DOM
    $('.modal-backdrop').remove();

    // Verificar si sigue existiendo algún modal abierto antes de eliminar la clase 'modal-open'
    if ($('.modal.show').length === 0) {
        $('body').removeClass('modal-open');
    }
}

function openModal(id) {
    //console.log('Abriendo Modal: ' + id);
    //$('#' + id).modal('show');

    // Asegúrate de que no queden backdrops anteriores
    cleanUpBackdrops();

    // Abre el modal después de asegurarse que está limpio
    $('#' + id).modal('show');
}

function cleanUpBackdrops() {
    // Asegúrate de cerrar cualquier modal abierto y eliminar los backdrop
    $('.modal-backdrop').remove();
    $('.modal.show').modal('hide');
    $('body').removeClass('modal-open');
}

//#region Canvas
function activeAllCanvases() {
    var canvases = document.getElementsByTagName('canvas');
    for (var i = 0; i < canvases.length; i++) {
        var canvas = canvases[i];
        console.log('Canvas ID: ' + canvas.id);
        canvas.addEventListener('mousedown', drawOnCanvas, false);
        canvas.addEventListener('mousemove', drawOnCanvas, false);
        canvas.addEventListener('mouseup', drawOnCanvas, false);
        canvas.addEventListener('mouseout', drawOnCanvas, false);
        canvas.addEventListener('touchstart', drawOnCanvas, false);
        canvas.addEventListener('touchmove', drawOnCanvas, false);
        canvas.addEventListener('touchend', drawOnCanvas, false);
        canvas.addEventListener('touchcancel', drawOnCanvas, false);
    }
}

function drawOnCanvas(event) {
    var canvas;
    if (event.type.indexOf('touch') !== -1) {
        // Evento táctil
        canvas = event.target;
        event.preventDefault();
    } else {
        // Evento de ratón
        canvas = this;
    }
    var ctx = canvas.getContext("2d");
    var rect = canvas.getBoundingClientRect();
    var mouseX, mouseY;

    if (event.type.indexOf('touch') !== -1) {
        // Evento táctil
        mouseX = event.touches[0].clientX - rect.left;
        mouseY = event.touches[0].clientY - rect.top;
    } else {
        // Evento de ratón
        mouseX = event.clientX - rect.left;
        mouseY = event.clientY - rect.top;
    }

    switch (event.type) {
        case 'mousedown':
        case 'touchstart':
            ctx.beginPath();
            ctx.moveTo(mouseX, mouseY);
            break;
        case 'mousemove':
        case 'touchmove':
            if (event.buttons === 1 || event.type.indexOf('touch') !== -1) {
                ctx.lineTo(mouseX, mouseY);
                ctx.stroke();
            }
            break;
        case 'mouseup':
        case 'mouseout':
        case 'touchend':
        case 'touchcancel':
            // No hacer nada en estos eventos
            break;
    }
}
//#endregion Canvas