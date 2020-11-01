var adriva;
(function (adriva) {
    var optimization;
    (function (optimization) {
        var assetType;
        (function (assetType) {
            assetType[assetType["javaScript"] = 1] = "javaScript";
            assetType[assetType["css"] = 2] = "css";
        })(assetType = optimization.assetType || (optimization.assetType = {}));
        var assetReference = /** @class */ (function () {
            function assetReference(path, contextName, type) {
                this.path = path;
                this.contextName = contextName;
                this.type = type;
            }
            return assetReference;
        }());
        var loader = /** @class */ (function () {
            function loader() {
            }
            Object.defineProperty(loader, "hasAssets", {
                get: function () {
                    return 0 < loader.assetPaths.length;
                },
                enumerable: true,
                configurable: true
            });
            loader.push = function (path, contextName, type) {
                if (!path)
                    return;
                if (!loader.isAttached) {
                    window.addEventListener('load', loader.loadAssets);
                    loader.isAttached = true;
                }
                var matchingElement = loader.assetPaths.filter(function (value) {
                    return value.type === type && value.path.toUpperCase() === path.toUpperCase() && value.contextName.toUpperCase() === contextName.toUpperCase();
                });
                if (0 < matchingElement.length)
                    return;
                loader.assetPaths.push(new assetReference(path, contextName, type));
            };
            loader.loadAssets = function () {
                var scriptAssets = loader.assetPaths.filter(function (value) {
                    return value.type == assetType.javaScript;
                });
                loader.loadScriptAssets(scriptAssets.reverse());
            };
            loader.loadScriptAssets = function (assets) {
                if (!assets || 0 == assets.length)
                    return;
                var assetReference = assets.pop();
                if (!assetReference)
                    return;
                var script = document.createElement("script");
                script.type = "text/javascript";
                script.onload = function () {
                    if (0 == assets.length) {
                        //load completed
                        loader.loadedContextNames.push(assetReference.contextName);
                        var contextReadyEvent = new CustomEvent('contextReady', { detail: assetReference.contextName });
                        document.dispatchEvent(contextReadyEvent);
                    }
                    loader.loadScriptAssets(assets);
                };
                script.src = assetReference.path;
                document.getElementsByTagName("head")[0].appendChild(script);
            };
            loader.isReady = function (contextName) {
                var readyNames = loader.loadedContextNames.filter(function (value) { return contextName.toUpperCase() === value.toUpperCase(); });
                return 1 == readyNames.length;
            };
            loader.isAttached = false;
            loader.assetPaths = [];
            loader.loadedContextNames = [];
            return loader;
        }());
        optimization.loader = loader;
    })(optimization = adriva.optimization || (adriva.optimization = {}));
})(adriva || (adriva = {}));
