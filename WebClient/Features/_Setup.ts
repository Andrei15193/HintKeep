import fs from "fs";
import path from "path";
import { After, AfterStep, Before, BeforeAll, setDefaultTimeout, setWorldConstructor } from "@cucumber/cucumber";
import { featureTestOptions } from "./_FeatureTestOptions";
import { JSDomWorld } from "./_JSDomWorld";

// CucumberJS Config
setWorldConstructor(JSDomWorld);
setDefaultTimeout(60 * 1000);

BeforeAll(async function () {
    if (featureTestOptions.failedScenarioHtmlSnapshotDirectoryPath)
        await createDirectoryAsync(featureTestOptions.failedScenarioHtmlSnapshotDirectoryPath);
    if (featureTestOptions.stepHtmlSnapshotDirectoryPath)
        await createDirectoryAsync(featureTestOptions.stepHtmlSnapshotDirectoryPath);
});

Before(function () {
    this.render();
});

AfterStep(async function (context) {
    if (featureTestOptions.stepHtmlSnapshotDirectoryPath)
        await writeFileAsync(
            path.join(
                featureTestOptions.stepHtmlSnapshotDirectoryPath,
                `${context.gherkinDocument.feature} - ${context.pickle.name} - ${context.pickleStep.text}.html`
            ),
            this.dom.serialize()
        );
});

After(async function (context) {
    if (
        context.result
        && (
            context.result.status === "FAILED"
            || context.result.status === "AMBIGUOUS"
        )
        && featureTestOptions.failedScenarioHtmlSnapshotDirectoryPath
    ) {
        this.document.head.title = `[${context.result.status}] ${this.document.head.title} (${context.pickle.name})`;

        await writeFileAsync(
            path.join(
                featureTestOptions.failedScenarioHtmlSnapshotDirectoryPath,
                `${context.gherkinDocument.feature?.name} - ${context.pickle.name}.html`
            ),
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

function createDirectoryAsync(path: string): Promise<void> {
    return new Promise<void>(
        (resolve, reject) => {
            fs.mkdir(path, { recursive: true }, (error) => {
                if (error)
                    reject(error);
                else
                    resolve();
            });
        }
    );
}

function writeFileAsync(path: string, content: string): Promise<void> {
    return new Promise<void>(
        (resolve, reject) => {
            fs.writeFile(path, content, (error) => {
                if (error)
                    reject(error);
                else
                    resolve();
            });
        }
    );
}