function createBarChart(elementId, labels, datasets) {
    var ctx = document.getElementById(elementId).getContext('2d');
    var chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createBarChart(elementId, title, labels, datasets) {
    var ctx = document.getElementById(elementId).getContext('2d');
    var chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: title
                }
            }
        }
    });
}

function createHorizontalBarChart(elementId, labels, datasets) {
    var ctx = document.getElementById(elementId).getContext('2d');
    var chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            indexAxis: 'y', // Esta propiedad cambia la orientación a horizontal
            scales: {
                x: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createLineChart(elementId, title, labels, datasets) {

    // Verificar si existe un gráfico asociado al canvas con el id proporcionado
    const canvas = document.getElementById(elementId);

    // Verificar si existe un gráfico asociado al canvas y destruirlo si es necesario
    if (canvas.chartInstance) {
        canvas.chartInstance.destroy();
        canvas.chartInstance = null;
    }

    var ctx = document.getElementById(elementId).getContext('2d');
    const newChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: title
                }
            }
        }
    });

    // Asociar la nueva instancia del gráfico al canvas
    canvas.chartInstance = newChart;
}

function createPieChart(elementId, title, labels, datasets) {

    // Verificar si existe un gráfico asociado al canvas con el id proporcionado
    const canvas = document.getElementById(elementId);

    // Verificar si existe un gráfico asociado al canvas y destruirlo si es necesario
    if (canvas.chartInstance) {
        canvas.chartInstance.destroy();
        canvas.chartInstance = null;
    }

    var ctx = document.getElementById(elementId).getContext('2d');
    const newChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: title
                }
            }
        }
    });

    // Asociar la nueva instancia del gráfico al canvas
    canvas.chartInstance = newChart;
}

function canvasReset(elementId) {

    // Verificar si existe un gráfico asociado al canvas con el id proporcionado
    const canvas = document.getElementById(elementId);

    // Verificar si existe un gráfico asociado al canvas y destruirlo si es necesario
    if (canvas.chartInstance) {
        canvas.chartInstance.destroy();
        canvas.chartInstance = null;
    }
}

function exportChart(elementId, format) {
    var canvas = document.getElementById(elementId);
    var ctx = canvas.getContext('2d');

    if (format === 'jpeg') {
        // Crear un nuevo canvas temporal con el mismo tamaño que el original
        var tempCanvas = document.createElement('canvas');
        var tempCtx = tempCanvas.getContext('2d');

        tempCanvas.width = canvas.width;
        tempCanvas.height = canvas.height;

        // Rellenar el fondo con blanco
        tempCtx.fillStyle = 'white';
        tempCtx.fillRect(0, 0, tempCanvas.width, tempCanvas.height);

        // Dibujar el contenido del canvas original sobre el fondo blanco
        tempCtx.drawImage(canvas, 0, 0);

        // Convertir el canvas temporal a una imagen JPG
        var link = document.createElement('a');
        link.href = tempCanvas.toDataURL(`image/jpeg`);
        link.download = `chart.jpg`;
        link.click();
    } else {
        // Para PNG u otros formatos, exportar directamente desde el canvas original
        var link = document.createElement('a');
        link.href = canvas.toDataURL(`image/${format}`);
        link.download = `chart.${format}`;
        link.click();
    }
}
