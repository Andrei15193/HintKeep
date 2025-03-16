import { HintKeepForm, HintKeepFormField } from "../../../Forms";
import { RequiredValidator } from "../../../Forms/Validation/required";

export class LoginForm extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.username = new HintKeepFormField<string>({
                name: "username",
                initialValue: "",
                validators: [new RequiredValidator("This field is mandatory. Please fill it in to login.")]
            }),
            this.password = new HintKeepFormField<string>({
                name: "password",
                initialValue: "",
                validators: [new RequiredValidator("This field is mandatory. Please fill it in to login.")]
            })
        );
    }

    public readonly username: HintKeepFormField<string>;
    public readonly password: HintKeepFormField<string>;
}