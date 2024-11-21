function initSortable(id, component) {
    var el = document.getElementById(id);
    if (el) {
        new Sortable(el, {
            animation: 200,
            group: "sortableGroup",
            onUpdate: function (event) {
                // Capture the new order and send it to Blazor
                const updatedIds = Array.from(el.children)
                    .map(child => {
                        const itemId = child.dataset.itemId;
                        console.log("Child dataset:", child.dataset); // Log the entire dataset for debugging
                        return itemId;
                    })
                    .filter(id => id !== undefined); // Filter out any undefined values
                console.log("Updated IDs: ", updatedIds);
                component.invokeMethodAsync('OnListReordered', updatedIds);
            }
        });
    } else {
        console.error(`Element with ID '${id}' not found.`);
    }
}


