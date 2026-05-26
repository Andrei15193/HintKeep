import { Given, When, Then, Before } from "@cucumber/cucumber";
import { fireEvent } from "@testing-library/react";
import { SignUpFormHandler } from "../Pages/SignUp/FormHandlers/SignUpFormHandler";
import { SignUpForm } from "../Pages/SignUp/Forms/SignUpForm";

Before(function () {
    this.render();
});

Given("the landing page", function () {
});

Given("I click on {string}", async function (text: string) {
    const element = await this.findByTextAsync(text);
    fireEvent.click(element);
});

Given("I see {string}", async function (text: string) {
    await this.findByTextAsync(text);
});

Given("there is an existing user with {string} username, {string} password and {string} hint", async function (username: string, password: string, hint: string) {
    const signUpForm = this.resolve(SignUpForm);

    signUpForm.username.value = username;
    signUpForm.password.value = password;
    signUpForm.hint.value = hint;

    await this
        .resolve(SignUpFormHandler)
        .handleAsync(signUpForm);
});

When("I enter {string} for {string}", async function (value: string, inputLabel: string) {
    const input = await this.findInputByLabelTextAsync(inputLabel);

    fireEvent.change(input, { target: { value } });
});

Then("I see the {string} error message for {string}", async function (error: string, inputLabel: string) {
    const labelElement = await this.findByTextAsync(inputLabel);

    await this.findByTextAsync(error, labelElement.parentElement);
});