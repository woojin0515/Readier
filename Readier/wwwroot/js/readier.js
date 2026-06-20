window.readierStorage = {
    getItem: function (key) {
        return window.localStorage.getItem(key);
    },
    setItem: function (key, value) {
        window.localStorage.setItem(key, value);
    },
    removeItem: function (key) {
        window.localStorage.removeItem(key);
    }
};

window.readierNotifications = {
    timers: {},
    isEnabled: function () {
        return "Notification" in window && Notification.permission === "granted";
    },
    requestPermission: async function () {
        if (!("Notification" in window)) {
            return false;
        }

        if (Notification.permission === "granted") {
            return true;
        }

        const permission = await Notification.requestPermission();
        return permission === "granted";
    },
    schedule: function (id, title, description, notifyTime) {
        if (!("Notification" in window)) {
            return;
        }

        const when = new Date(notifyTime);
        const delay = when.getTime() - Date.now();
        if (delay <= 0) {
            return;
        }

        window.readierNotifications.cancel(id);
        window.readierNotifications.timers[id] = window.setTimeout(() => {
            if (Notification.permission === "granted") {
                new Notification(title, { body: description });
            }
            delete window.readierNotifications.timers[id];
        }, delay);
    },
    cancel: function (id) {
        const timer = window.readierNotifications.timers[id];
        if (timer) {
            window.clearTimeout(timer);
            delete window.readierNotifications.timers[id];
        }
    },
    cancelGroup: function (...ids) {
        ids.forEach(id => window.readierNotifications.cancel(id));
    }
};

window.readierMap = {
    render: function (element, origin, destination) {
        if (!element || typeof L === "undefined") {
            return;
        }

        let state = element._readierMapState;
        if (!state) {
            const map = L.map(element, {
                zoomControl: false,
                attributionControl: true
            });

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(map);

            state = {
                map: map,
                markers: [],
                line: null
            };

            element._readierMapState = state;
        }

        state.markers.forEach(marker => marker.remove());
        state.markers = [];
        if (state.line) {
            state.line.remove();
            state.line = null;
        }

        const points = [];

        const addPoint = (place, color) => {
            if (!place || place.latitude === 0 && place.longitude === 0) {
                return;
            }

            const marker = L.circleMarker([place.latitude, place.longitude], {
                radius: 9,
                color: color,
                fillColor: color,
                fillOpacity: 0.95,
                weight: 2
            }).addTo(state.map);

            marker.bindPopup(`<strong>${place.label ?? ""}</strong><br/>${place.address ?? ""}`);
            state.markers.push(marker);
            points.push([place.latitude, place.longitude]);
        };

        addPoint(origin, "#5b7cfa");
        addPoint(destination, "#f59e0b");

        if (points.length === 2) {
            state.line = L.polyline(points, {
                color: "#5b7cfa",
                weight: 3,
                opacity: 0.75
            }).addTo(state.map);
            state.map.fitBounds(points, { padding: [24, 24] });
        } else if (points.length === 1) {
            state.map.setView(points[0], 14);
        } else {
            state.map.setView([37.5665, 126.9780], 11);
        }

        state.map.invalidateSize(true);
    },
    destroy: function (element) {
        const state = element && element._readierMapState;
        if (!state) {
            return;
        }

        if (state.line) {
            state.line.remove();
        }

        state.markers.forEach(marker => marker.remove());
        state.map.remove();
        delete element._readierMapState;
    }
};
