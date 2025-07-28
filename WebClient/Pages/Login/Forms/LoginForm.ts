import { RequiredValidator } from "../../../Core/Forms/Validation";
import { HintKeepForm, HintKeepFormField } from "../../../Core/Forms/ViewModels";

export class LoginForm extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.username = new HintKeepFormField<string>({
                name: "username",
                label: "Username",
                initialValue: "",
                validators: [new RequiredValidator("This field is mandatory. Please fill it in to login.")]
            }),
            this.password = new HintKeepFormField<string>({
                name: "password",
                label: "Password",
                initialValue: "",
                validators: [new RequiredValidator("This field is mandatory. Please fill it in to login.")]
            })
        );
    }

    public readonly username: HintKeepFormField<string>;
    public readonly password: HintKeepFormField<string>;
}