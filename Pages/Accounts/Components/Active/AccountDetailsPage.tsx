import React from "react";
import { ViewEditToggle } from "../../../../Core/ViewEditToggle";
import { AccountDetailsPageEdit } from "./AccountDetailsPageEdit";
import { AccountDetailsPageView } from "./AccountDetailsPageView";

export function ActiveAccountDetailsPage(): React.JSX.Element {
    return (
        <ViewEditToggle
            viewComponent={AccountDetailsPageView}
            editComponent={AccountDetailsPageEdit}
        />
    );
}