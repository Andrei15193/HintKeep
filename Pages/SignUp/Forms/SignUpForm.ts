import { HintKeepForm, HintKeepFormField } from "../../../Forms";
import { RequiredValidator } from "../../../Forms/Validation/required";

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
            this.hint = new HintKeepFormField<string>({
                name: "hint",
                label: "Hint",
                initialValue: "",
                validators: [new RequiredValidator()]
            })
        );
    }

    public readonly username: HintKeepFormField<string>;
    public readonly password: HintKeepFormField<string>;
    public readonly hint: HintKeepFormField<string>;
}