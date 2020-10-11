namespace adriva {
    export namespace optimization {

        export enum assetType {
            javaScript = 1,
            css = 2
        }

        class assetReference {
            constructor(public path: string, public contextName: string, public type: assetType) {

            }
        }

        export class loader {
            private static isAttached: boolean = false;
            private static assetPaths: Array<assetReference> = [];
            private static loadedContextNames: Array<string> = [];

            public static push(path: string, contextName: string, type: assetType): void {
                if (!path) return;
                if (!loader.isAttached) {
                    window.addEventListener('load', loader.loadAssets);
                    loader.isAttached = true;
                }

                var matchingElement = loader.assetPaths.filter((value) => {
                    return value.type === type && value.path.toUpperCase() === path.toUpperCase() && value.contextName.toUpperCase() === contextName.toUpperCase();
                });

                if (0 < matchingElement.length) return;
                loader.assetPaths.push(new assetReference(path, contextName, type));
            }

            private static loadAssets() {
                var scriptAssets = loader.assetPaths.filter((value) => {
                    return value.type == assetType.javaScript
                });

                loader.loadScriptAssets(scriptAssets.reverse());
            }

            private static loadScriptAssets(assets: Array<assetReference>): void {
                if (!assets || 0 == assets.length) return;

                var assetReference = assets.pop();
                if (!assetReference) return;
                var script = document.createElement("script")
                script.type = "text/javascript";

                script.onload = function () {
                    if (0 == assets.length) {
                        //load completed
                        loader.loadedContextNames.push(assetReference.contextName);
                        let contextReadyEvent = new CustomEvent('contextReady', { detail: assetReference.contextName });
                        document.dispatchEvent(contextReadyEvent);
                    }
                    loader.loadScriptAssets(assets);
                };

                script.src = assetReference.path;
                document.getElementsByTagName("head")[0].appendChild(script);
            }

            public static isReady(contextName: string): boolean {
                var readyNames = loader.loadedContextNames.filter((value) => contextName.toUpperCase() === value.toUpperCase());
                return 1 == readyNames.length;
            }
        }
    }
}
