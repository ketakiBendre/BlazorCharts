function initSortable(id, component) {
    var el = document.getElementById(id);
    if (el) {
        new Sortable(el, {
            animation: 200,
            group: "sortableGroup"
        });
    } else {
        console.error(`Element with ID '${id}' not found.`);
    }
}

