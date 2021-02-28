import type { IFormField } from '../../../view-models/core';
import Style from '../../style.scss';

export function getValidationClasses(field: IFormField): { [className: string]: boolean } {
    return {
        [Style.isValid]: field.isTouched && field.isValid,
        [Style.isInvalid]: field.isTouched && field.isInvalid
    }
}