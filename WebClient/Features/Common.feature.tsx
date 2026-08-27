import Assert from "assert";
import { Given, When, Then } from "@cucumber/cucumber";
import { fireEvent } from "@testing-library/react";
import { CurrentUser } from "../Core/Authentication";
import { SignUpRoute } from "../Pages/SignUp";
import { IndexedDbSignUpFormHandler } from "../Pages/SignUp/FormHandlers/IndexedDbSignUpFormHandler";
import { SignUpForm } from "../Pages/SignUp/Forms/SignUpForm";

Given("the landing page", function () {
});

Given("the sign up page", async function () {
    await this.router.navigate(SignUpRoute.path!);
    await this.findByTextAsync("HintKeep - Sign Up");
});

Given("I click on {string}", async function (text: string) {
    const element = await this.findByTextAsync(text);
    fireEvent.click(element);
});

Given("I see {string}", async function (text: string) {
    await this.findByTextAsync(text);
});

Given("there is an existing user with {string} username, {string} password, {string} hint and {string} email", async function (username: string, password: string, hint: string, email: string) {
    await this.submitFormAsync(IndexedDbSignUpFormHandler, SignUpForm, {
        username,
        password,
        passwordConfirmation: password,
        hint,
        email
    });
});

When("I fill the {string} field with {int} characters", async function (inputLabel: string, characterCount: number) {
    const input = await this.findInputByLabelTextAsync(inputLabel);

    fireEvent.change(input, { target: { value: "a".repeat(characterCount) } });
});

When("I enter {string} for {string}", async function (value: string, inputLabel: string) {
    const input = await this.findInputByLabelTextAsync(inputLabel);

    fireEvent.change(input, { target: { value } });
});

When("I click on the {string} link", async function (buttonText: string) {
    const element = await this.findByTextAsync(buttonText);
    Assert.equal(element.tagName, "A");

    fireEvent.click(element);
});

When("I click on the {string} button", async function (buttonText: string) {
    let element = await this.findByTextAsync(buttonText);

    if (element.tagName === "SPAN" && element.parentElement)
        element = element.parentElement;
    Assert.equal(element.tagName, "BUTTON");

    fireEvent.click(element);
});

Then("I see the {string} page", async function (pageTitle: string) {
    await this.findByTextAsync(`HintKeep - ${pageTitle}`);
});

Then("I see the {string} field", async function (inputLabel: string) {
    const element = await this.findByTextAsync(inputLabel);
    Assert.equal(element.tagName, "LABEL");
});

Then("I see the {string} button", async function (buttonText: string) {
    let element = await this.findByTextAsync(buttonText);
    if (element.tagName === "SPAN" && element.parentElement)
        element = element.parentElement;

    Assert.equal(element.tagName, "BUTTON");
});

Then("I see the {string} link", async function (linkText: string) {
    let element = await this.findByTextAsync(linkText);

    Assert.equal(element.tagName, "A");
});

Then("I see {string} error message for the {string} field", async function (error: string, inputLabel: string) {
    const labelElement = await this.findByTextAsync(inputLabel);

    await this.findByTextAsync(error, labelElement.parentElement);
});

Then("the current user is {string}", async function (username: string) {
    const currentUser = this.dependencyResolver.resolve(CurrentUser);

    Assert.equal(currentUser.username, username);
});