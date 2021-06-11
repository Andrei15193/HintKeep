import type { IFormFieldViewModel } from 'react-model-view-viewmodel';
import Style from '../../style.scss';

export function getValidationClasses(field: IFormFieldViewModel<any>): { [className: string]: boolean } {
    return {
        [Style.isValid]: field.isTouched && field.isValid,
        [Style.isInvalid]: field.isTouched && field.isInvalid
    }
}