import React from "react";
import { ViewEditToggle } from "../ViewEditToggle";
import { AccountDetailsPageEdit } from "./AccountDetailsPageEdit";
import { AccountDetailsPageView } from "./AccountDetailsPageView";

export function AccountDetailsPage(): React.JSX.Element {
    return (
        <ViewEditToggle
            viewComponent={AccountDetailsPageView}
            editComponent={AccountDetailsPageEdit}
        />
    );
}