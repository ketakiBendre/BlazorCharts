window.initializeMapbox = (latitude, longitude, zoom) => {
    mapboxgl.accessToken = 'pk.eyJ1IjoicWZzZXJ2aWNlIiwiYSI6ImNtNXZlZXFuajAxMW4yanE2NXlkYzVjYjQifQ.yt3RAlNbu7pOWYH2imLdJA';

    var map = new mapboxgl.Map({
        container: 'map', // ID of the div where the map will be rendered
        style: 'mapbox://styles/mapbox/streets-v11', // Choose the style
        center: [longitude, latitude], // Center map on the coordinates
        zoom: zoom // Zoom level
    });

    // Optionally, you can add a marker to the searched location
    new mapboxgl.Marker()
        .setLngLat([longitude, latitude])
        .addTo(map);
};

function initializeMapboxWithRoute(locations) {
    mapboxgl.accessToken = 'pk.eyJ1IjoicWZzZXJ2aWNlIiwiYSI6ImNtNXZlZXFuajAxMW4yanE2NXlkYzVjYjQifQ.yt3RAlNbu7pOWYH2imLdJA';

    var map = new mapboxgl.Map({
        container: 'map',
        style: 'mapbox://styles/mapbox/streets-v11',
        center: [locations[0].Longitude, locations[0].Latitude],
        zoom: 12
    });

    // Add zoom controls
    map.addControl(new mapboxgl.NavigationControl());

    let routeCoordinates = locations.map(location => [location.Longitude, location.Latitude]);

    if (routeCoordinates.length > 1) {
        map.on('load', function () {
            // Add the route source and layer
            map.addSource('route', {
                type: 'geojson',
                data: {
                    type: 'Feature',
                    geometry: {
                        type: 'LineString',
                        coordinates: routeCoordinates
                    }
                }
            });

            map.addLayer({
                id: 'route',
                type: 'line',
                source: 'route',
                layout: {
                    'line-join': 'round',
                    'line-cap': 'round'
                },
                paint: {
                    'line-color': '#ff0000',
                    'line-width': 4
                }
            });

            // Add Start Marker (Green)
            new mapboxgl.Marker({ color: 'green' })
                .setLngLat(routeCoordinates[0])
                .setPopup(new mapboxgl.Popup().setText('Start Point')) // Optional label
                .addTo(map);

            // Add End Marker (Red)
            new mapboxgl.Marker({ color: 'red' })
                .setLngLat(routeCoordinates[routeCoordinates.length - 1])
                .setPopup(new mapboxgl.Popup().setText('End Point')) // Optional label
                .addTo(map);

            // Zoom to fit the route
            const bounds = routeCoordinates.reduce((bounds, coord) => bounds.extend(coord), new mapboxgl.LngLatBounds(routeCoordinates[0], routeCoordinates[0]));
            map.fitBounds(bounds, {
                padding: 50
            });
        });
    }
}



