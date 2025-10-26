import { setDefaultTimeout, setWorldConstructor } from "@cucumber/cucumber";
import { JSDomWorld } from "./_JSDomWorld";

// CucumberJS Config
setWorldConstructor(JSDomWorld);
setDefaultTimeout(60 * 1000);

// Declare the context globally
declare module "@cucumber/cucumber" {
    interface IWorld extends JSDomWorld {
    }
}

declare global {
    var window: Window & typeof globalThis;
    var document: Document;
}