import type { IMessages } from './IMessages';

export const enGB: IMessages = {
    'pages.header.banner': '- HintKeep -',
    'pages.footer.banner': 'HintKeep - Store hints, not passwords.',

    'pages.accounts.pageTitle': 'Accounts',
    'pages.accounts.noAccounts': 'You do not currently have any accounts :(',
    'pages.accounts.add.label': 'Add',
    'pages.accounts.edit.label': 'Edit',
    'pages.accounts.viewAccountsBin.labels': 'View accounts (bin)',

    'pages.deletedAccounts.pageTitle': 'Accounts (bin)',
    'pages.deletedAccounts.noAccounts': 'There are no account in the bin',
    'pages.deletedAccounts.view.label': 'View',
    'pages.deletedAccounts.viewAccounts.labels': 'View accounts',

    'pages.addAccount.pageTitle': 'Add account',
    'pages.addAccount.name.label': 'Name',
    'pages.addAccount.name.placeholder': 'HintKeep',
    'pages.addAccount.hint.label': 'Hint',
    'pages.addAccount.hint.placeholder': 'Crazy person in a box',
    'pages.addAccount.isPinned.label': 'Is pinned',
    'pages.addAccount.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.addAccount.add.label': 'Add',
    'pages.addAccount.cancel.label': 'Cancel',

    'pages.editAccount.pageTitle': 'Edit account',
    'pages.editAccount.name.label': 'Name',
    'pages.editAccount.name.placeholder': 'HintKeep',
    'pages.editAccount.hint.label': 'Hint',
    'pages.editAccount.hint.placeholder': 'Crazy person in a box',
    'pages.editAccount.isPinned.label': 'Is pinned',
    'pages.editAccount.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.editAccount.save.label': 'Save',
    'pages.editAccount.delete.label': 'Delete',
    'pages.editAccount.moveToBin.label': 'Move to bin',
    'pages.editAccount.delete.confirmationModalTitle': 'Confirm deletion',
    'pages.editAccount.delete.confirmation': 'The account will be moved to the bin where it can still be viewed and restored. To completely remove the account you need to delete it from there as well.',
    'pages.editAccount.cancel.label': 'Cancel',

    'pages.deletedAccountDetails.pageTitle': 'Account',
    'pages.deletedAccountDetails.name.label': 'Name',
    'pages.deletedAccountDetails.hint.label': 'Hint',
    'pages.deletedAccountDetails.isPinned.label': 'Is pinned',
    'pages.deletedAccountDetails.isPinned.description': 'Pinned accounts are shown at the top for more visibility.',
    'pages.deletedAccountDetails.restore.label': 'Restore',
    'pages.deletedAccountDetails.delete.label': 'Delete',
    'pages.deletedAccountDetails.delete.confirmationModalTitle': 'Confirm deletion',
    'pages.deletedAccountDetails.delete.confirmation': 'The account will be permanently deleted and you will no longer be able to recover it! Please confirm your operation.',
    'pages.deletedAccountDetails.cancel.label': 'Cancel',

    'validation.errors.required': 'Please provide a value.',
    'validation.errors.nameNotUnique': 'An account with the same name already exists.',

    'errors.internalServerError': 'Something went wrong with the server. Please refresh the page and try again.',
    'errors.accountNotFound': 'The account you are looking for does not exist.'
};