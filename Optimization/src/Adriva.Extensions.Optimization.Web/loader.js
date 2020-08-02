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
            function assetReference(path, type) {
                this.path = path;
                this.type = type;
            }
            return assetReference;
        }());
        var loader = /** @class */ (function () {
            function loader() {
            }
            loader.push = function (path, type) {
                if (!path)
                    return;
                if (!loader.isAttached) {
                    window.addEventListener('load', loader.loadAssets);
                    loader.isAttached = true;
                }
                var matchingElement = loader.assetPaths.filter(function (value) {
                    return value.type === type && value.path.toUpperCase() === path.toUpperCase();
                });
                if (0 < matchingElement.length)
                    return;
                loader.assetPaths.push(new assetReference(path, type));
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
                var script = document.createElement("script");
                script.type = "text/javascript";
                script.onload = function () {
                    loader.loadScriptAssets(assets);
                };
                script.src = assets.pop().path;
                document.getElementsByTagName("head")[0].appendChild(script);
            };
            loader.isAttached = false;
            loader.assetPaths = [];
            return loader;
        }());
        optimization.loader = loader;
    })(optimization = adriva.optimization || (adriva.optimization = {}));
})(adriva || (adriva = {}));
