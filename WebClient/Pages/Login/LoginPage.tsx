import React, { useEffect } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { Link } from "react-router";
import { UserHandler } from "../../Core/Authentication";
import { Form, Button } from "../../Core/Forms/Components";
import { FormField, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { FormFieldGroup } from "../../Core/Forms/Components/FormFields/FormField";
import { useFormFlow } from "../../Core/PageFlows";
import { Content, Header } from "../../Core/PageParts";
import { DropDatabaseModal } from "./Components/DropDatabaseModal";
import { IndexDbLoginFormHandler } from "./FormHandlers/IndexDbLoginFormHandler";
import { LoginForm } from "./Forms/LoginForm";

export function LoginPage(): React.JSX.Element {
    const userHandler = useDependency(UserHandler);

    const {
        form,
        result: user,
        isSubmitting: isLoggingIn,
        isSubmitted: isLoggedIn,
        submitAsync: logInAsync
    } = useFormFlow({
        form: LoginForm,
        formHandler: IndexDbLoginFormHandler
    });

    useEffect(
        () => {
            if (isLoggedIn)
                userHandler.authenticate(user);
        },
        [isLoggedIn, user, userHandler]
    );

    return (
        <>
            <Header>
                HintKeep - Login
            </Header>

            <Content>
                <p>
                    Welcome to HintKeep! Currently, we only have the option to sign up and store your hints locally, in the browser. Don't worry, we save the data if you close it!
                </p>

                <Form isLoading={isLoggingIn}>
                    <FormFieldGroup>
                        <FormField field={form.username}>
                            <FormFieldLabel />
                            <FormFieldTextInput placeholder="name1983" />
                        </FormField>

                        <FormField field={form.password}>
                            <FormFieldLabel />
                            <FormFieldTextInput
                                type="password"
                                placeholder="admin"
                            />
                        </FormField>
                    </FormFieldGroup>

                    <div className="toolbar">
                        <Button
                            type="submit"
                            text="Login"
                            disabled
                            processing={isLoggingIn}
                        />
                        <Button
                            type="submit"
                            text="Login with LocalDB"
                            processing={isLoggingIn}
                            onClick={logInAsync}
                        />
                        <Link to="/sign-up">
                            Sign up
                        </Link>

                        {
                            HINTKEEP_ENVIRONMENT_TYPE === "development"
                            && <DropDatabaseModal />
                        }
                    </div>
                </Form>
            </Content>
        </>
    );
}