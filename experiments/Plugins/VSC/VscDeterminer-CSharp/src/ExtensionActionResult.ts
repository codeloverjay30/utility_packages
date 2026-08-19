import { ExtensionAction } from "./ExtensionAction";

/**
    * Represents the result of an extension action execution.
 */
export interface ExtensionActionResult {
    readonly extensionId: string;
    readonly action: ExtensionAction;
    readonly status: ExtensionActionStatus;
    readonly message: string;
}