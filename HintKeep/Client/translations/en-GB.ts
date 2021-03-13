import type { IMessages } from './IMessages';

export const enGB: IMessages = {
    'pages.header.banner': '- HintKeep -',
    'pages.footer.banner': 'HintKeep - Store hints, not passwords.',

    'pages.signUp.pageTitle': 'Sign-Up',
    'pages.signUp.email.label': 'E-Mail address',
    'pages.signUp.email.placeholder': 'name@example.com',
    'pages.signUp.password.label': 'Password',
    'pages.signUp.password.description': 'The password must be at least 8 characters long, contain upper case and lower case letters, at least one digit and at least one non alphanumeric character.',
    'pages.signUp.submit.label': 'Sign-Up',
    'pages.signUp.cancel.label': 'Cancel',

    'pages.userConfirmation.pageTitle': 'User Confirmation',
    'pages.userConfirmation.email.label': 'E-Mail address',
    'pages.userConfirmation.email.placeholder': 'name@example.com',
    'pages.userConfirmation.token.label': 'Confirmation token',
    'pages.userConfirmation.token.description': 'A confirmation token has been sent to the provided e-mail address, just copy and paste it here.',
    'pages.userConfirmation.submit.label': 'Confirm',
    'pages.userConfirmation.cancel.label': 'Cancel',

    'pages.login.pageTitle': 'Login',
    'pages.login.email.label': 'E-Mail address',
    'pages.login.email.placeholder': 'name@example.com',
    'pages.login.password.label': 'Password',
    'pages.login.submit.label': 'Login',
    'pages.login.signUp.label': 'Sign-Up',
    'pages.login.recoverUserAccount.label': 'Recover account',

    'validation.errors.required': 'Please provide a value.',
    'validation.errors.emailNotUnique': 'The provided e-mail cannot be used to create an account (already in use).',
    'validation.errors.invalidCredentials': 'The provided e-mail/password are not valid.',
    'validation.errors.invalidEmail': 'The provided value is not a valid e-mail address.',
    'validation.errors.invalidPassword': 'The provided password does not meet the security constraints.',
    'validation.errors.invalidSignUpConfirmationToken': 'The provided token is no longer valid. Please sign-up again.',

    'errors.internalServerError': 'Something went wrong with the server. Please refresh the page and try again.'
};