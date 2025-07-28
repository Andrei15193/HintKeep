import type { IAccountDetails } from "../Models/IAcountDetails";
import { MaxLengthValidator, RequiredValidator } from "../../../Core/Forms/Validation";
import { HintKeepForm, HintKeepFormField } from "../../../Core/Forms/ViewModels";

export class AccountForm extends HintKeepForm {
    public constructor(account?: IAccountDetails | null) {
        super();

        this.id = account?.id || null;
        this.withFields(
            this.name = new HintKeepFormField<string>({
                name: "name",
                label: "Account",
                initialValue: account?.name || "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.username = new HintKeepFormField<string>({
                name: "username",
                label: "Username",
                initialValue: account?.username || "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.hint = new HintKeepFormField<string>({
                name: "hint",
                label: "Hint",
                initialValue: account?.hint || "",
                validators: [new RequiredValidator(), new MaxLengthValidator(250)]
            }),
            this.pinned = new HintKeepFormField<boolean>({
                name: "pinned",
                label: "Is pinned",
                initialValue: account?.isPinned || false
            }),
            this.notes = new HintKeepFormField<string>({
                name: "notes",
                label: "Notes",
                initialValue: account?.notes || "",
                validators: [new MaxLengthValidator(1000)]
            })
        );
    }

    public readonly id: string | null;
    public readonly archived: boolean;
    public readonly name: HintKeepFormField<string>;
    public readonly username: HintKeepFormField<string>;
    public readonly hint: HintKeepFormField<string>;
    public readonly pinned: HintKeepFormField<boolean>;
    public readonly notes: HintKeepFormField<string>;
}