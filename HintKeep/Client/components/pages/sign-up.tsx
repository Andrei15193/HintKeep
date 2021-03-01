import type { IObserver } from '../../observer';
import React, { useEffect } from 'react';
import { useHistory } from "react-router-dom";
import classnames from 'classnames';
import { Link } from 'react-router-dom';
import { BusyContent } from '../loaders';
import { WithViewModel } from '../view-model-wrappers';
import { SignUpViewModel } from '../../view-models/sign-up-view-model';
import { FormInput } from './forms';

import Style from '../style.scss';

export function SignUp(): JSX.Element {
    return (
        <WithViewModel viewModelType={SignUpViewModel}>{($vm) => {
            const history = useHistory();
            useEffect(() => $vm.submittedEvent.subscribeWithCallback(() => history.push('/user-confirmations')), [$vm]);

            return <>
                <h1 className={Style.textCenter}>Sign-Up</h1>
                <BusyContent $vm={$vm}>
                    <FormInput className={Style.mb3} id="email" type="text" label="Email address" field={$vm.email} placeholder="name@example.com" />
                    <FormInput className={Style.mb3} id="password" type="password" label="Password" field={$vm.password} />
                    <div>
                        <button type="submit" disabled={$vm.isInvalid} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>Submit</button>
                        <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>Cancel</Link>
                    </div>
                </BusyContent>
            </>;
        }}</WithViewModel>
    );
}