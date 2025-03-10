import { HintKeepForm, HintKeepFormField } from "../../../Forms";
import { required } from "../../../Forms/Validation/required";

export class LoginForm extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.username = new HintKeepFormField<string>({
                name: "username",
                initialValue: "",
                validators: [required]
            }),
            this.password = new HintKeepFormField<string>({
                name: "password",
                initialValue: "",
                validators: [required]
            })
        );
    }

    public readonly username: HintKeepFormField<string>;
    public readonly password: HintKeepFormField<string>;
}