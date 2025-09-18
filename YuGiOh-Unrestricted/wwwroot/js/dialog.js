document.addEventListener('click', function (e) {
    const dlg = document.querySelector('dialog[open]');
    if (!dlg) return;
    const rect = dlg.getBoundingClientRect();
    const inside = e.clientX >= rect.left && e.clientX <= rect.right && e.clientY >= rect.top && e.clientY <= rect.bottom;
    if (inside) return;
    const url = new URL(window.location.href);
    url.searchParams.delete('details');
    history.replaceState(null, '', url.toString());
    dlg.close();
});
