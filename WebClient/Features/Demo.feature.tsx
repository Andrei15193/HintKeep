import assert from "assert";
import { Given, When, Then } from "@cucumber/cucumber";
import { findByText } from "@testing-library/react";
import React from "react";
import { Startup } from "../Pages/Startup";

Given("the landing page", async function () {
    this.render(<Startup />);
});

When("I look at the page", () => {
});

Then("I see a \"Use application locally\" link", async function () {
    const element = await findByText(this.container, "Use application locally", {
        collapseWhitespace: true,
        exact: true
    });

    assert.equal(element.tagName, "A");
    assert.equal(element.attributes.getNamedItem("href")?.value, "/login");
});