import { type IValidator, type ValidatorCallback, type IFormFieldConfig, FormField } from "react-model-view-viewmodel";

export interface IHintKeepFormFieldConfig<TValue> extends IFormFieldConfig<TValue> {
    readonly wasTouched?: boolean;

    readonly validators?: readonly (IValidator<HintKeepFormField<TValue>> | ValidatorCallback<HintKeepFormField<TValue>>)[];
}

export class HintKeepFormField<TValue> extends FormField<TValue> {
    private _wasTouched: boolean;

    public constructor(fieldConfig: IHintKeepFormFieldConfig<TValue>) {
        super(fieldConfig);

        this.wasTouched = !!fieldConfig.wasTouched;
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
}