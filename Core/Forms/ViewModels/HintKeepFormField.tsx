import { type IValidator, type ValidatorCallback, type IFormFieldConfig, FormField } from "react-model-view-viewmodel";

export interface IHintKeepFormFieldConfig<TValue> extends IFormFieldConfig<TValue> {
    readonly label: string;
    readonly wasTouched?: boolean;

    readonly validators?: readonly (IValidator<HintKeepFormField<TValue>> | ValidatorCallback<HintKeepFormField<TValue>>)[];
}

export class HintKeepFormField<TValue> extends FormField<TValue> {
    private _label: string;
    private _wasTouched: boolean;
    private _isRequired: boolean;

    public constructor(fieldConfig: IHintKeepFormFieldConfig<TValue>) {
        super(fieldConfig);

        this.label = fieldConfig.label;
        this.wasTouched = !!fieldConfig.wasTouched;
        this._isRequired = !!this._isRequired;
    }

    public get label(): string {
        return this._label;
    }

    public set label(value: string) {
        if (this._label !== value) {
            this._label = value;
            this.notifyPropertiesChanged("label");
        }
    }

    public get wasTouched(): boolean {
        return this._wasTouched;
    }

    public set wasTouched(value: boolean) {
        if (this._wasTouched !== value) {
            this._wasTouched = value;
            this.notifyPropertiesChanged("wasTouched");
        }
    }

    public get isRequired(): boolean {
        return this._isRequired;
    }

    public set isRequired(value: boolean) {
        if (this._isRequired !== value) {
            this._isRequired = value;
            this.notifyPropertiesChanged("isRequired");
        }
    }
}