import { When } from "@cucumber/cucumber";
import { fireEvent } from "@testing-library/react";
import { StorageContext } from "../Core/Data/StorageContext";

When("I click on the login button", async function () {
    const storageContext = this.dependencyResolver.resolve(StorageContext);

    const element = await this.findByTextAsync(storageContext.storageType === "IndexedDB" ? "Login with LocalDB" : "Login");
    fireEvent.click(element);
});