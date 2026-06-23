// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


console.log('hello world!');
const parser = new DOMParser();

function addGuest(name) {
    const tmpl = document.getElementById('tmpl-new-guest');
    if (tmpl == null) {
        return;
    }

    let numExisting = document.querySelectorAll('.rsvp .guest').length;

    let html = tmpl.innerHTML;
    html = html.replaceAll('{{i}}', numExisting + 1);
    html = html.replaceAll('{{name}}', name);

    let div = parser.parseFromString(html, 'text/html');

    const target = document.getElementById('guest-list');
    target.append(div.body.firstChild);
}


document.addEventListener('DOMContentLoaded', () => {
    const btnAddGuest = document.getElementById('add-guest');
    const input = document.getElementById('new-guest-block');

    if (btnAddGuest) {
        btnAddGuest.addEventListener('click', (e) => {
            input.classList.remove('d-none');
        });
    }

    const saveNewGuest = document.getElementById('save-new-guest');
    if (saveNewGuest) {
        saveNewGuest.addEventListener('click', (e) => {
            const name = document.getElementById('new-guest').value;
            addGuest(name);
            input.classList.add('d-none');
        });
    }
});