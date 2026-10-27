// Alterna un campo de contraseña entre oculto/visible.
// Uso: <button type="button" onclick="alternarContrasena('miInput', this)">👁️</button>
function alternarContrasena(idInput, boton) {
    var input = document.getElementById(idInput);
    if (!input) return;
    if (input.type === 'password') {
        input.type = 'text';
        boton.textContent = '🙈';
        boton.setAttribute('aria-label', 'Ocultar contraseña');
    } else {
        input.type = 'password';
        boton.textContent = '👁️';
        boton.setAttribute('aria-label', 'Mostrar contraseña');
    }
}
