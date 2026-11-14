const calificaciones = [];
let token = sessionStorage.getItem("token");
let rolSesion = sessionStorage.getItem("rol");
let nombreSesion = sessionStorage.getItem("nombre");

const formulario = document.getElementById("formulario-calificacion");
const tablaCalificaciones = document.getElementById("tabla-calificaciones");
const mensaje = document.getElementById("mensaje");

document.getElementById("formulario-login").addEventListener("submit", async function (evento) {
    evento.preventDefault();
    try {
        const respuesta = await fetch("/api/Auth/Login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                usuario: document.getElementById("usuario").value,
                clave: document.getElementById("clave").value
            })
        });
        if (!respuesta.ok) {
            throw new Error("Usuario o clave incorrectos.");
        }

        const datos = await respuesta.json();
        token = datos.token;
        rolSesion = datos.rol;
        nombreSesion = datos.nombre;
        sessionStorage.setItem("token", token);
        sessionStorage.setItem("rol", rolSesion);
        sessionStorage.setItem("nombre", nombreSesion);
        mostrarContenido(true);
        mostrarMensaje("Sesión iniciada.");
        if (rolSesion === "Estudiante") consultarResultados();
        if (rolSesion === "Docente") cargarResultadosDocente();
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
});

document.getElementById("boton-salir").addEventListener("click", function () {
    cerrarSesion();
    mostrarMensaje("Sesión cerrada.");
});

document.getElementById("formulario-estudiante").addEventListener("submit", async function (evento) {
    evento.preventDefault();
    const estudiante = {
        idEstudiante: document.getElementById("codigo-estudiante").value.trim(),
        nombre: document.getElementById("nombre-estudiante").value.trim(),
        usuario: document.getElementById("usuario-estudiante").value.trim(),
        clave: document.getElementById("clave-estudiante").value
    };

    try {
        const respuesta = await fetch("/api/Estudiantes/Registrar", {
            method: "POST",
            headers: encabezadosAutorizados({ "Content-Type": "application/json" }),
            body: JSON.stringify(estudiante)
        });
        if (!respuesta.ok) {
            throw new Error(await respuesta.text() || "No se pudo crear la cuenta.");
        }
        evento.target.reset();
        mostrarMensaje("Cuenta de estudiante creada.");
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
});

formulario.addEventListener("submit", function (evento) {
    evento.preventDefault();
    const calificacion = {
        idEstudiante: document.getElementById("id-estudiante").value.trim(),
        idExposicion: Number(document.getElementById("id-exposicion").value),
        nombreExposicion: document.getElementById("nombre-exposicion").value.trim(),
        nota: Number(document.getElementById("nota").value)
    };

    if (calificacion.idEstudiante === "" || calificacion.idExposicion < 1 ||
        calificacion.nombreExposicion === "" || calificacion.nota < 0 || calificacion.nota > 5) {
        mostrarMensaje("Ingresa el código del estudiante, la exposición y una nota entre 0 y 5.", true);
        return;
    }

    calificaciones.push(calificacion);
    agregarFila(tablaCalificaciones, [calificacion.idEstudiante, calificacion.idExposicion, calificacion.nombreExposicion, calificacion.nota]);
    formulario.reset();
    mostrarMensaje("Calificación agregada.");
});

document.getElementById("boton-consolidar").addEventListener("click", async function () {
    if (calificaciones.length === 0) {
        mostrarMensaje("Agrega al menos una calificación antes de consolidar.", true);
        return;
    }

    try {
        const respuesta = await fetch("/api/Resultados/Consolidar", {
            method: "POST",
            headers: encabezadosAutorizados({ "Content-Type": "application/json" }),
            body: JSON.stringify(calificaciones)
        });
        if (respuesta.status === 401) {
            salirPorTokenVencido();
            throw new Error("La sesión venció. Inicia sesión de nuevo.");
        }
        if (!respuesta.ok) {
            throw new Error(await respuesta.text() || "No se pudieron consolidar los resultados.");
        }

        mostrarMensaje("Resultados consolidados correctamente.");
        calificaciones.length = 0;
        tablaCalificaciones.replaceChildren();
        cargarResultadosDocente();
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
});

document.getElementById("boton-consultar").addEventListener("click", consultarResultados);

async function cargarResultadosDocente() {
    try {
        const respuesta = await fetch("/api/Resultados", { headers: encabezadosAutorizados() });
        if (respuesta.status === 401) {
            salirPorTokenVencido();
            throw new Error("La sesión venció. Inicia sesión de nuevo.");
        }
        if (!respuesta.ok) {
            throw new Error("No se pudieron cargar los resultados consolidados.");
        }

        mostrarResultados(await respuesta.json(), "Docente");
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

async function consultarResultados() {
    try {
        const respuesta = await fetch("/api/Resultados/Consultar", { headers: encabezadosAutorizados() });
        if (respuesta.status === 401) {
            salirPorTokenVencido();
            throw new Error("La sesión venció. Inicia sesión de nuevo.");
        }
        if (!respuesta.ok) {
            throw new Error("No se pudieron consultar tus resultados.");
        }

        mostrarResultados(await respuesta.json(), "Estudiante");
        mostrarMensaje("Tus resultados están actualizados.");
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

function mostrarResultados(resultados, rol) {
    const tabla = document.getElementById(rol === "Docente" ? "tabla-resultados-docente" : "tabla-resultados-estudiante");
    tabla.replaceChildren();

    if (resultados.length === 0) {
        const fila = document.createElement("tr");
        const celda = document.createElement("td");
        celda.colSpan = rol === "Docente" ? 4 : 3;
        celda.textContent = "Todavía no hay resultados consolidados para mostrar.";
        fila.appendChild(celda);
        tabla.appendChild(fila);
        return;
    }

    resultados.forEach(function (resultado) {
        const valores = rol === "Docente"
            ? [resultado.idEstudiante, resultado.nombreExposicion, resultado.promedio.toFixed(2), resultado.cantidadEvaluaciones]
            : [resultado.nombreExposicion, resultado.promedio.toFixed(2), resultado.cantidadEvaluaciones];
        agregarFila(tabla, valores);
    });
}

function agregarFila(tabla, valores) {
    const fila = document.createElement("tr");
    valores.forEach(function (valor) {
        const celda = document.createElement("td");
        celda.textContent = valor;
        fila.appendChild(celda);
    });
    tabla.appendChild(fila);
}

function mostrarMensaje(texto, esError = false) {
    mensaje.textContent = texto;
    mensaje.classList.toggle("error", esError);
}

function encabezadosAutorizados(encabezados = {}) {
    return { ...encabezados, Authorization: `Bearer ${token}` };
}

function mostrarContenido(autenticado) {
    document.getElementById("seccion-login").hidden = autenticado;
    document.getElementById("contenido-principal").hidden = !autenticado;
    document.getElementById("panel-docente").hidden = !autenticado || rolSesion !== "Docente";
    document.getElementById("panel-estudiante").hidden = !autenticado || rolSesion !== "Estudiante";
    document.getElementById("usuario-actual").textContent = autenticado ? `${nombreSesion} · ${rolSesion}` : "";
}

function cerrarSesion() {
    token = null;
    rolSesion = null;
    nombreSesion = null;
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("rol");
    sessionStorage.removeItem("nombre");
    mostrarContenido(false);
}

function salirPorTokenVencido() {
    cerrarSesion();
}

mostrarContenido(Boolean(token && rolSesion && nombreSesion));
if (token && rolSesion === "Estudiante") consultarResultados();
if (token && rolSesion === "Docente") cargarResultadosDocente();
