import React from 'react';
import classnames from 'classnames';
import { Link, useHistory } from 'react-router-dom';
import { BusyContent } from '../loaders';
import { WithViewModel, useEvent } from '../view-model-wrappers';
import { SignUpViewModel } from '../../view-models/sign-up-view-model';
import { FormInput } from './forms';

import Style from '../style.scss';

export function SignUp(): JSX.Element {
    return (
        <WithViewModel viewModelType={SignUpViewModel}>{($vm) => {
            const history = useHistory();
            useEvent($vm.submittedEvent, () => history.push('/user-confirmations'));

            return <>
                <h1 className={Style.textCenter}>Sign-Up</h1>
                <BusyContent $vm={$vm}>
                    <FormInput className={Style.mb3} id="email" type="text" label="Email address" field={$vm.email} placeholder="name@example.com" />
                    <FormInput className={Style.mb3} id="password" type="password" label="Password" field={$vm.password} />
                    <div>
                        <button type="submit" disabled={($vm.isInvalid && $vm.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>Submit</button>
                        <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>Cancel</Link>
                    </div>
                </BusyContent>
            </>;
        }}</WithViewModel>
    );
}