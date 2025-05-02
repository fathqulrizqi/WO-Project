function Ajax() {
    var Component = (function(options, complete) {
        if (Array.isArray(options)) {
            // Create multiple request controller
            var multiple = {
                instance: [],
                complete: complete,
            }

            if (options.length > 0) {
                for (var i = 0; i < options.length; i++) {
                    options[i].multiple = multiple;
                    multiple.instance.push(Component(options[i]));
                }
            }

            return multiple;
        }

        if (! options.data) {
            options.data = {};
        }

        if (options.type) {
            options.method = options.type;
        }

        // Default method
        if (! options.method) {
            options.method = 'GET';
        }

        // Default type
        if (! options.dataType) {
            options.dataType = 'json';
        }

        if (options.data) {
            // Parse object to variables format
            var parseData = function (value, key) {
                var vars = [];
                if (value) {
                    var keys = Object.keys(value);
                    if (keys.length) {
                        for (var i = 0; i < keys.length; i++) {
                            if (key) {
                                var k = key + '[' + keys[i] + ']';
                            } else {
                                var k = keys[i];
                            }

                            if (value[k] instanceof FileList) {
                                vars[k] = value[keys[i]];
                            } else if (value[keys[i]] === null || value[keys[i]] === undefined) {
                                vars[k] = '';
                            } else if (typeof(value[keys[i]]) == 'object') {
                                var r = parseData(value[keys[i]], k);
                                var o = Object.keys(r);
                                for (var j = 0; j < o.length; j++) {
                                    vars[o[j]] = r[o[j]];
                                }
                            } else {
                                vars[k] = value[keys[i]];
                            }
                        }
                    }
                }

                return vars;
            }

            var d = parseData(options.data);
            var k = Object.keys(d);

            // Data form
            if (options.method == 'GET') {
                if (k.length) {
                    var data = [];
                    for (var i = 0; i < k.length; i++) {
                        data.push(k[i] + '=' + encodeURIComponent(d[k[i]]));
                    }

                    if (options.url.indexOf('?') < 0) {
                        options.url += '?';
                    }
                    options.url += data.join('&');
                }
            } else {
                var data = new FormData();
                for (var i = 0; i < k.length; i++) {
                    if (d[k[i]] instanceof FileList) {
                        if (d[k[i]].length) {
                            for (var j = 0; j < d[k[i]].length; j++) {
                                data.append(k[i], d[k[i]][j], d[k[i]][j].name);
                            }
                        }
                    } else {
                        data.append(k[i], d[k[i]]);
                    }
                }
            }
        }

        var httpRequest = new XMLHttpRequest();
        httpRequest.open(options.method, options.url, true);
        httpRequest.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        // Content type
        if (options.contentType) {
            httpRequest.setRequestHeader('Content-Type', options.contentType);
        }

        // Headers
        if (options.method === 'POST') {
            httpRequest.setRequestHeader('Accept', 'application/json');
        } else {
            if (options.dataType === 'blob') {
                httpRequest.responseType = "blob";
            } else {
                if (! options.contentType) {
                    if (options.dataType === 'json') {
                        httpRequest.setRequestHeader('Content-Type', 'text/json');
                    } else if (options.dataType === 'html') {
                        httpRequest.setRequestHeader('Content-Type', 'text/html');
                    }
                }
            }
        }

        // No cache
        if (options.cache !== true) {
            httpRequest.setRequestHeader('pragma', 'no-cache');
            httpRequest.setRequestHeader('cache-control', 'no-cache');
        }

        // Authentication
        if (options.withCredentials === true) {
            httpRequest.withCredentials = true
        }

        // Before send
        if (typeof(options.beforeSend) == 'function') {
            options.beforeSend(httpRequest);
        }

        // Before send
        if (typeof(Component.beforeSend) == 'function') {
            Component.beforeSend(httpRequest);
        }

        if (document.ajax && typeof(document.ajax.beforeSend) == 'function') {
            document.ajax.beforeSend(httpRequest);
        }

        httpRequest.onload = function() {
            if (httpRequest.status >= 200 && httpRequest.status < 300) {
                if (options.dataType === 'json') {
                    try {
                        var result = JSON.parse(httpRequest.responseText);

                        if (options.success && typeof(options.success) == 'function') {
                            options.success(result);
                        }
                    } catch(err) {
                        if (options.error && typeof(options.error) == 'function') {
                            options.error(err, result);
                        }
                    }
                } else {
                    if (options.dataType === 'blob') {
                        var result = httpRequest.response;
                    } else {
                        var result = httpRequest.responseText;
                    }

                    if (options.success && typeof(options.success) == 'function') {
                        options.success(result);
                    }
                }
            } else {
                if (options.error && typeof(options.error) == 'function') {
                    options.error(httpRequest.responseText, httpRequest.status);
                }
            }

            // Global queue
            if (Component.queue && Component.queue.length > 0) {
                Component.send(Component.queue.shift());
            }

            // Global complete method
            if (Component.requests && Component.requests.length) {
                // Get index of this request in the container
                var index = Component.requests.indexOf(httpRequest);
                // Remove from the ajax requests container
                Component.requests.splice(index, 1);
                // Deprecated: Last one?
                if (! Component.requests.length) {
                    // Object event
                    if (options.complete && typeof(options.complete) == 'function') {
                        options.complete(result);
                    }
                }
                // Group requests
                if (options.group) {
                    if (Component.oncomplete && typeof(Component.oncomplete[options.group]) == 'function') {
                        if (! Component.pending(options.group)) {
                            Component.oncomplete[options.group]();
                            Component.oncomplete[options.group] = null;
                        }
                    }
                }
                // Multiple requests controller
                if (options.multiple && options.multiple.instance) {
                    // Get index of this request in the container
                    var index = options.multiple.instance.indexOf(httpRequest);
                    // Remove from the ajax requests container
                    options.multiple.instance.splice(index, 1);
                    // If this is the last one call method complete
                    if (! options.multiple.instance.length) {
                        if (options.multiple.complete && typeof(options.multiple.complete) == 'function') {
                            options.multiple.complete(result);
                        }
                    }
                }
            }
        }

        // Keep the options
        httpRequest.options = options;
        // Data
        httpRequest.data = data;

        // Queue
        if (options.queue === true && Component.requests.length > 0) {
            Component.queue.push(httpRequest);
        } else {
            Component.send(httpRequest)
        }

        return httpRequest;
    });

    Component.send = function(httpRequest) {
        if (httpRequest.data) {
            if (Array.isArray(httpRequest.data)) {
                httpRequest.send(httpRequest.data.join('&'));
            } else {
                httpRequest.send(httpRequest.data);
            }
        } else {
            httpRequest.send();
        }

        Component.requests.push(httpRequest);
    }

    Component.exists = function(url, __callback) {
        var http = new XMLHttpRequest();
        http.open('HEAD', url, false);
        http.send();
        if (http.status) {
            __callback(http.status);
        }
    }

    Component.pending = function(group) {
        var n = 0;
        var o = Component.requests;
        if (o && o.length) {
            for (var i = 0; i < o.length; i++) {
                if (! group || group == o[i].options.group) {
                    n++
                }
            }
        }
        return n;
    }

    Component.oncomplete = {};
    Component.requests = [];
    Component.queue = [];

    return Component
}

export default Ajax();