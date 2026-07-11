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
    setStatus: function (elementId, message, isError) {
        if (!elementId) {
            return;
        }

        const element = document.getElementById(elementId);
        if (!element) {
            return;
        }

        if (!message) {
            element.hidden = true;
            element.textContent = "";
            element.classList.remove("error");
            element.classList.add("info");
            return;
        }

        element.hidden = false;
        element.textContent = message;
        element.classList.toggle("error", !!isError);
        element.classList.toggle("info", !isError);
    },
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
    requestPermissionFromGesture: async function (statusElementId) {
        if (!("Notification" in window)) {
            window.readierNotifications.setStatus(statusElementId, "이 브라우저는 알림을 지원하지 않습니다.", true);
            return false;
        }

        if (Notification.permission === "granted") {
            window.readierNotifications.setStatus(statusElementId, "이미 알림 권한이 허용되어 있습니다.", false);
            return true;
        }

        const permission = await Notification.requestPermission();
        if (permission === "granted") {
            window.readierNotifications.setStatus(statusElementId, "알림 권한이 허용되었습니다.", false);
            return true;
        }

        if (permission === "denied") {
            window.readierNotifications.setStatus(statusElementId, "브라우저에서 알림이 차단되어 있습니다. 사이트 설정에서 허용해 주세요.", true);
            return false;
        }

        window.readierNotifications.setStatus(statusElementId, "알림 권한 요청이 취소되었습니다.", true);
        return false;
    },
    showNow: function (title, description) {
        if (!("Notification" in window)) {
            return false;
        }

        if (Notification.permission !== "granted") {
            return false;
        }

        new Notification(title, { body: description });
        return true;
    },
    previewFromGesture: async function (useCalmCopy, statusElementId) {
        const granted = await window.readierNotifications.requestPermissionFromGesture(statusElementId);
        if (!granted) {
            return false;
        }

        const title = useCalmCopy ? "이제 천천히 준비를 시작해 볼까요?" : "준비 시작 시간이에요";
        const description = "이렇게 표시됩니다.";
        new Notification(title, { body: description });
        window.readierNotifications.setStatus(statusElementId, "알림 미리보기를 표시했습니다.", false);
        return true;
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
