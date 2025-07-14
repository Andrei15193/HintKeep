import type { HintKeepFormField } from "../Forms/ViewModels";
import { DependencyToken } from "react-model-view-viewmodel";

export const AccountsSearchTextFieldToken = new DependencyToken<HintKeepFormField<string>>("accounts-search-text");
export const ArchivedAccountsSearchTextFieldToken = new DependencyToken<HintKeepFormField<string>>("archived-accounts-search-text");