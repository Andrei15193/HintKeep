import React from 'react';
import classnames from 'classnames';

import Style from './style.scss';

enum ComponentLoaderState {
    Loading,
    Ready,
    Faulted
}

function delayAsync(millisecondsDelay: number): Promise<undefined> {
    return new Promise(resolve => setTimeout(() => resolve(undefined), millisecondsDelay));
}

export interface IComponentLoaderProps<TCompProps = {}> {
    loadAsync(): Promise<React.ComponentType<TCompProps>>
}

export interface IComponentLoaderState<TCompProps = {}> {
    readonly component?: React.ComponentType<TCompProps>
    readonly state: ComponentLoaderState
}

abstract class BaseComponentLoader<TCompProps = {}, TOwnProps = {}> extends React.Component<TCompProps & TOwnProps, IComponentLoaderState<TCompProps>> {
    public constructor(props: TCompProps & TOwnProps) {
        super(props);
        this.state = {
            component: undefined,
            state: ComponentLoaderState.Loading
        };
    }

    public componentDidMount(): void {
        delayAsync(3000)
            .then(() => this.loadAsync())
            .then(component => this.setState({ component, state: ComponentLoaderState.Ready }))
            .catch(() => this.setState({ state: ComponentLoaderState.Faulted }));
    }

    public render(): JSX.Element | null {
        const { component: Component, state } = this.state;

        switch (state) {
            case ComponentLoaderState.Loading:
                return (
                    <div className={classnames(Style.dFlex, Style.justifyContentCenter)}>
                        <div className={classnames(Style.spinnerBorder, Style.my3)} style={{ width: "3rem", height: "3rem" }} role="status"></div>
                    </div>
                );

            case ComponentLoaderState.Faulted:
                return (
                    <div className={classnames(Style.alert, Style.alertDanger)} role="alert">
                        Failed to load component. <a href="" className={Style.alertLink} onClick={() => document.location.reload()}>Reload</a>
                    </div>
                );

            default:
                return (Component !== undefined ? React.createElement(Component, this.props) : null);
        };
    }

    protected abstract loadAsync(): Promise<React.ComponentType<TCompProps>>;
}

export function getComponentLoader<TCompProps>(loadAsync: () => Promise<React.ComponentType<TCompProps>>): React.ComponentType<TCompProps> {
    return class ComponentLoader extends BaseComponentLoader<TCompProps> {
        public constructor(props: TCompProps) {
            super(props);
        }

        protected loadAsync(): Promise<React.ComponentType<TCompProps>> {
            return loadAsync();
        }
    };
}

export class ComponentLoader extends BaseComponentLoader<{}, IComponentLoaderProps> {
    public constructor(props: IComponentLoaderProps) {
        super(props);
    }

    protected loadAsync(): Promise<React.ComponentType> {
        return this.props.loadAsync();
    }
}