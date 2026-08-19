import { ExtensionAction } from "./ExtensionAction";
import { ExtensionActionScope } from "./ExtensionActionScope";
import { VscEnvironmentEvent } from "./ExtensionRule";

/**
    * Represents a branch of a rule for determining the action to be taken for a specific extension based on the configuration.
    @param action - The action to be taken for the extension (Install, Enable, Disable, Ignore, Default).
    @param on - An array of VscEnvironmentEvent values that define the conditions under which this branch applies.
    @param scope - The scope in which the action should be applied (User, Workspace, etc.).
    @param installWhenMissingOrCorrupted - A flag indicating whether to install the extension if it is missing or corrupted.
 */
export interface ExtensionRuleBranch {
    readonly action: ExtensionAction;

    readonly on: readonly VscEnvironmentEvent[];

    readonly scope: ExtensionActionScope;

    readonly installWhenMissingOrCorrupted: boolean;
}