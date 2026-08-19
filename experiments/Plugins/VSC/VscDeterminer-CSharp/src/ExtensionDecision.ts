import { ExtensionAction } from "./ExtensionAction";
import { ExtensionActionScope } from "./ExtensionActionScope";
import { VscEnvironmentEvent } from "./ExtensionRule";

/**
    * Represents a decision for an extension based on the configuration.
    @param extensionId - The unique identifier of the extension.
    @param action - The action to be taken for the extension (Install, Enable, Disable, Ignore, Default).
    @param scope - The scope in which the action should be applied (User, Workspace, etc.).
    @param installWhenMissingOrCorrupted - A flag indicating whether to install the extension if it is missing or corrupted.
    @param matchedEvent - An optional event that triggered this decision.
 */
export interface ExtensionDecision {
    readonly extensionId: string;
    readonly action: ExtensionAction;
    readonly scope: ExtensionActionScope;

    readonly installWhenMissingOrCorrupted: boolean;

    readonly matchedEvent?: VscEnvironmentEvent;
}