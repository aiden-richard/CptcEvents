(function (window, document) {
    'use strict';

    if (window.CalendarWidget) {
        return;
    }

    var CDN_SRC = 'https://cdn.jsdelivr.net/npm/fullcalendar@6.1.19/index.global.min.js';
    var CDN_SCRIPT_ID = 'fullcalendar-cdn-script';

    function ensureFullCalendarLoaded(onLoaded) {
        if (window.FullCalendar) {
            onLoaded();
            return;
        }

        var script = document.getElementById(CDN_SCRIPT_ID);
        if (!script) {
            script = document.createElement('script');
            script.id = CDN_SCRIPT_ID;
            script.src = CDN_SRC;
            script.async = true;
            script.addEventListener('load', onLoaded);
            script.addEventListener('error', function () {
                console.error('Failed to load FullCalendar from CDN.');
            });
            document.head.appendChild(script);
            return;
        }

        script.addEventListener('load', onLoaded);
    }

    function fetchEvents(eventsUrl, success, failure) {
        fetch(eventsUrl)
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Network response was not ok: ' + response.status);
                }
                return response.json();
            })
            .then(function (eventsArray) {
                if (Array.isArray(eventsArray)) {
                    success(eventsArray);
                } else {
                    success([]);
                }
            })
            .catch(function (error) {
                console.error('Error fetching or parsing events:', error);
                if (typeof failure === 'function') {
                    failure(error);
                }
            });
    }

    function initCalendar(el, options) {
        if (!el) {
            return;
        }

        var initialView = options.initialView || 'dayGridMonth';
        var eventsUrl = options.eventsUrl;
        if (!eventsUrl) {
            console.error('Calendar requires an eventsUrl.');
            return;
        }

        ensureFullCalendarLoaded(function () {
            if (!window.FullCalendar) {
                console.error('FullCalendar failed to load.');
                return;
            }

            var calendar = new FullCalendar.Calendar(el, {
                initialView: initialView,
                events: function (info, successCallback, failureCallback) {
                    fetchEvents(eventsUrl, successCallback, failureCallback);
                }
            });

            calendar.render();
        });
    }

    function initFromElement(elementId) {
        var el = typeof elementId === 'string' ? document.getElementById(elementId) : elementId;
        if (!el) {
            console.warn('Calendar element not found for id:', elementId);
            return;
        }

        var options = {
            initialView: el.dataset.initialView || 'dayGridMonth',
            eventsUrl: el.dataset.eventsUrl || ''
        };

        initCalendar(el, options);
    }

    // Expose a tiny API for reuse across pages.
    window.CalendarWidget = {
        init: initCalendar,
        initFromElement: initFromElement
    };
})(window, document);
