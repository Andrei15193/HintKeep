import { HintKeepForm, HintKeepFormField } from "../../../Forms";
import { MaxLengthValidator } from "../../../Forms/Validation/maxLength";
import { RequiredValidator } from "../../../Forms/Validation/required";

export class AccountForm extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.name = new HintKeepFormField<string>({
                name: "name",
                label: "Account",
                initialValue: "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.username = new HintKeepFormField<string>({
                name: "username",
                label: "Username",
                initialValue: "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.hint = new HintKeepFormField<string>({
                name: "hint",
                label: "Hint",
                initialValue: "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.pinned = new HintKeepFormField<boolean>({
                name: "pinned",
                label: "Is pinned",
                initialValue: false
            }),
            this.notes = new HintKeepFormField<string>({
                name: "notes",
                label: "Notes",
                initialValue: "",
                validators: [new MaxLengthValidator(1000)]
            })
        );
    }

    public readonly name: HintKeepFormField<string>;
    public readonly username: HintKeepFormField<string>;
    public readonly hint: HintKeepFormField<string>;
    public readonly pinned: HintKeepFormField<boolean>;
    public readonly notes: HintKeepFormField<string>;
}