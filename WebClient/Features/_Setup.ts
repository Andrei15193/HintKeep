import fs from "fs";
import path from "path";
import { After, Before, setDefaultTimeout, setWorldConstructor } from "@cucumber/cucumber";
import { featureTestOptions } from "./_FeatureTestOptions";
import { JSDomWorld } from "./_JSDomWorld";

// CucumberJS Config
setWorldConstructor(JSDomWorld);
setDefaultTimeout(60 * 1000);

Before(function () {
    this.render();
});

After(function (context) {
    if (featureTestOptions.htmlSnapshotDirectoryPath) {
        if (context.result)
            this.document.head.title = `[${context.result.status}] ${this.document.head.title} (${context.pickle.name})`;

        fs.mkdirSync(featureTestOptions.htmlSnapshotDirectoryPath, { recursive: true });

        fs.writeFileSync(
            path.join(featureTestOptions.htmlSnapshotDirectoryPath, context.pickle.name + " snapshot.html"),
            this.dom.serialize()
        );
    }
});

// Declare the context globally
declare module "@cucumber/cucumber" {
    interface IWorld extends JSDomWorld {
    }
}

declare global {
    var window: Window & typeof globalThis;
    var document: Document;
}