import { Before } from "@cucumber/cucumber";
import { StorageContext } from "../Core/Data/StorageContext";

Before(function () {
    const storageContext = this.dependencyResolver.resolve(StorageContext);
    storageContext.useHintKeepApi();
});