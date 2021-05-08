import type { IMessages } from './IMessages';

export const enGB: IMessages = {
    'pages.header.banner': '- HintKeep -',
    'pages.footer.banner': 'HintKeep - Store hints, not passwords.',

    'pages.terms.pageTitle': 'Terms of Service',
    'pages.terms.lastModified': 'Last modified: 1st May, 2021',
    'pages.terms.paragraph.1': 'Welcome to HintKeep, we are happy to have you. Please read our terms of service below to know the benefits and limitations of using this application/website.',
    'pages.terms.paragraph.2': 'We may remove your account from our system if you are not following these terms of service! Please read carefully as registering an account automatically means you have read and understood these them.',
    'pages.terms.paragraph.3': 'This application/website is available to everyone, of any age, free of charge. There are no advertisements displayed here and never will be, neither will we ever charge people for using this service.',
    'pages.terms.list.1.item.1': 'We reserve the right to make any changes to the terms of service, at any moment.',
    'pages.terms.list.1.item.2': 'We reserve the right to shut down the website without any prior notice.',
    'pages.terms.list.1.item.3': 'We maintain a list of registered accounts (e-mail and password) that are solely used to authenticate users and, if needed, contact them directly, nothing more. We will not sell your personal data or expose it in any way.',
    'pages.terms.list.1.item.4': 'We cannot be held liable if a user’s personal data is exposed due to their own actions, the user is responsible for maintaining their account secure with a strong password, not sharing it with anyone as well as keeping their system clean from any malicious software (such as key loggers).',
    'pages.terms.list.1.item.5': 'We reserve the right to block any account if they have violated the terms of service.',
    'pages.terms.list.1.item.6': 'This application/website is available for use for both commercial and non-commercial purposes (available to everyone).',
    'pages.terms.list.1.item.7': 'We cannot be held liable when using 3rd party applications or libraries for accessing the application/website or its API. Data leaks can happen through these 3rd party applications, please be considerate of what you use.',
    'pages.terms.list.1.item.8.text.1': 'You can remove your account at any given moment by sending a request as ',
    'pages.terms.list.1.item.8.text.2': ' containing in the subject the nature of the request and in the body a short description, in the English language, of the request and the e-mail address identifying the account.',
    'pages.terms.list.1.item.9.text.1': 'Any suggestions to improve this application/website are welcomed and can be sent to ',
    'pages.terms.list.1.item.9.text.2': ', describe your suggestions in the English language, thank you!',

    'pages.accounts.pageTitle': 'Accounts',
    'pages.accounts.noAccounts': 'You do not currently have any accounts :(',
    'pages.accounts.search.placeholder': 'Search',
    'pages.accounts.add.label': 'Add',
    'pages.accounts.edit.label': 'Edit',
    'pages.accounts.extra.label': '. . .',
    'pages.accounts.viewAccountsBin.labels': 'View accounts (bin)',

    'pages.deletedAccounts.pageTitle': 'Accounts (bin)',
    'pages.deletedAccounts.noAccounts': 'There are no account in the bin',
    'pages.deletedAccounts.view.label': 'View',
    'pages.deletedAccounts.viewAccounts.labels': 'View accounts',
    'pages.deletedAccounts.back.label': 'Back',

    'pages.addAccount.pageTitle': 'Add account',
    'pages.addAccount.name.label': 'Name',
    'pages.addAccount.name.placeholder': 'HintKeep',
    'pages.addAccount.hint.label': 'Hint',
    'pages.addAccount.hint.placeholder': 'Crazy person in a box',
    'pages.addAccount.isPinned.label': 'Is pinned',
    'pages.addAccount.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.addAccount.notes.label': 'Notes',
    'pages.addAccount.add.label': 'Add',
    'pages.addAccount.cancel.label': 'Cancel',
    'pages.addAccount.back.label': 'Back',

    'pages.editAccount.pageTitle': 'Edit account',
    'pages.editAccount.name.label': 'Name',
    'pages.editAccount.name.placeholder': 'HintKeep',
    'pages.editAccount.hint.label': 'Hint',
    'pages.editAccount.hint.placeholder': 'Crazy person in a box',
    'pages.editAccount.isPinned.label': 'Is pinned',
    'pages.editAccount.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.editAccount.notes.label': 'Notes',
    'pages.editAccount.viewAllHints.label': 'View all hints',
    'pages.editAccount.save.label': 'Save',
    'pages.editAccount.delete.label': 'Delete',
    'pages.editAccount.moveToBin.label': 'Move to bin',
    'pages.editAccount.delete.confirmationModalTitle': 'Confirm deletion',
    'pages.editAccount.delete.confirmation': 'The account will be moved to the bin where it can still be viewed and restored. To completely remove the account you need to delete it from there as well.',
    'pages.editAccount.cancel.label': 'Cancel',
    'pages.editAccount.back.label': 'Back',

    'pages.accountHints.pageTitle': 'Hints',
    'pages.accountHints.back.label': 'Back',
    'pages.accountHints.delete.label': 'Delete',
    'pages.accountHints.dateAdded': 'Date added {dateAdded}',
    'pages.accountHints.noAccountHints': 'There are currently no hints for this account :(',
    'pages.accountHints.delete.confirmationModalTitle': 'Confirm deletion',
    'pages.accountHints.delete.confirmation': 'The hint will be permanently deleted, please confirm your action.',
    'pages.accountHints.cancel.label': 'Cancel',

    'pages.deletedAccountDetails.pageTitle': 'Account',
    'pages.deletedAccountDetails.name.label': 'Name',
    'pages.deletedAccountDetails.hint.label': 'Hint',
    'pages.deletedAccountDetails.isPinned.label': 'Is pinned',
    'pages.deletedAccountDetails.notes.label': 'Notes',
    'pages.deletedAccountDetails.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.deletedAccountDetails.restore.label': 'Restore',
    'pages.deletedAccountDetails.delete.label': 'Delete',
    'pages.deletedAccountDetails.delete.confirmationModalTitle': 'Confirm deletion',
    'pages.deletedAccountDetails.delete.confirmation': 'The account will be permanently deleted and you will no longer be able to recover it! Please confirm your operation.',
    'pages.deletedAccountDetails.cancel.label': 'Cancel',
    'pages.deletedAccountDetails.back.label': 'Back',

    'pages.extra.pageTitle': 'Extra',
    'pages.extra.back.label': 'Back',
    'pages.extra.accountsBin.label': 'View deleted accounts (bin)',
    'pages.extra.logOut.label': 'Log out of your account',

    'validation.errors.required': 'Please provide a value.',
    'validation.errors.nameNotUnique': 'An account with the same name already exists.',

    'errors.internalServerError': 'Something went wrong with the server. Please refresh the page and try again.',
    'errors.accountNotFound': 'The account you are looking for does not exist.'
};