import { type IValidator, type ValidatorCallback, type IFormFieldConfig, FormField } from "react-model-view-viewmodel";
import { type EqualityComparer, areValuesEqual } from "../../Comparison";

export interface IHintKeepFormFieldConfig<TValue> extends IFormFieldConfig<TValue> {
    readonly label: string;
    readonly wasTouched?: boolean;

    readonly validators?: readonly (IValidator<HintKeepFormField<TValue>> | ValidatorCallback<HintKeepFormField<TValue>>)[];

    hasChanged?: EqualityComparer<TValue>;
}

export class HintKeepFormField<TValue> extends FormField<TValue> {
    private _label: string;
    private _wasTouched: boolean;
    private _isRequired: boolean;
    private _hasChanged: boolean;
    private _hasChangedCallback: EqualityComparer<any>;

    public constructor(fieldConfig: IHintKeepFormFieldConfig<TValue>) {
        super(fieldConfig);

        const {
            label,
            wasTouched = false,
            hasChanged = areValuesEqual
        } = fieldConfig;

        this.label = label;
        this.wasTouched = wasTouched;
        this._isRequired = !!this._isRequired;

        this._hasChangedCallback = hasChanged;
        this._hasChanged = this._hasChangedCallback(this.value, this.initialValue);
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

    public get hasChanged(): boolean {
        return this._hasChanged;
    }

    public override get value(): TValue {
        return super.value;
    }

    public override set value(value: TValue) {
        super.value = value;

        const hasChanged = this._hasChangedCallback(this.value, this.initialValue);
        if (this._hasChanged !== hasChanged) {
            this._hasChanged = hasChanged;
            this.notifyPropertiesChanged("hasChanged");
        }
    }

    public override get initialValue(): TValue {
        return super.initialValue;
    }

    public override set initialValue(value: TValue) {
        super.initialValue = value;

        const hasChanged = this._hasChangedCallback(this.value, this.initialValue);
        if (this._hasChanged !== hasChanged) {
            this._hasChanged = hasChanged;
            this.notifyPropertiesChanged("hasChanged");
        }
    }
}