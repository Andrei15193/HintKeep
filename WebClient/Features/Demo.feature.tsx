import { Given, When, Then } from "@cucumber/cucumber";
import { fireEvent } from "@testing-library/react";
import React from "react";
import { Startup } from "../Pages/Startup";

Given("the landing page", async function () {
    this.render(<Startup />);
});

Given("I click on {string}", async function (text: string) {
    const element = await this.findByTextAsync(text);
    fireEvent.click(element);
});

Given("I see {string}", async function (text: string) {
    await this.findByTextAsync(text);
});

When("I enter {string} for {string}", async function (value: string, inputLabel: string) {
    const input = await this.findInputByLabelTextAsync(inputLabel);

    fireEvent.change(input, { target: { value } });
});

Then("I see the {string} error message for {string}", async function (error: string, inputLabel: string) {
    const labelElement = await this.findByTextAsync(inputLabel);

    await this.findByTextAsync(error, labelElement.parentElement);
});