import { RequiredValidator } from "../../../Core/Forms/Validation";
import { HintKeepForm, HintKeepFormField } from "../../../Core/Forms/ViewModels";

export class SignUpForm extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.username = new HintKeepFormField<string>({
                name: "username",
                label: "Username",
                initialValue: "",
                validators: [new RequiredValidator()]
            }),
            this.password = new HintKeepFormField<string>({
                name: "password",
                label: "Password",
                initialValue: "",
                validators: [new RequiredValidator()]
            }),
            this.passwordConfirmation = new HintKeepFormField<string>({
                name: "password-confirmation",
                label: "Password Confirmation",
                initialValue: "",
                validators: [
                    new RequiredValidator("A matching password is required"),
                    (field) => (field.value !== this.password.value ? "A matching password is required" : undefined)
                ],
                validationTriggers: [this.password]
            }),
            this.hint = new HintKeepFormField<string>({
                name: "hint",
                label: "Hint",
                initialValue: "",
                validators: [new RequiredValidator()]
            }),
            this.email = new HintKeepFormField<string>({
                name: "email",
                label: "E-Mail",
                initialValue: "",
                validators: [new RequiredValidator()]
            })
        );
    }

    public readonly username: HintKeepFormField<string>;
    public readonly password: HintKeepFormField<string>;
    public readonly passwordConfirmation: HintKeepFormField<string>;
    public readonly hint: HintKeepFormField<string>;
    public readonly email: HintKeepFormField<string>;
}