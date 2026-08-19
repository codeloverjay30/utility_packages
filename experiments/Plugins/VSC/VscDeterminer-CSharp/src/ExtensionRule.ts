import {
    ExtensionAction
} from './ExtensionAction';

import { ExtensionRuleBranch } from './ExtensionRuleBranch';

export type VscEnvironmentEvent =
    | 'vsc-opened'
    | 'vsc-workspace-opened';

/**
    * Represents a rule for determining the action to be taken for a specific extension based on the configuration.
    * @param extensionId - The unique identifier of the extension.
    * @param branches - An array of ExtensionRuleBranch objects that define the conditions and actions for the extension.
    * @param defaultAction - The default action to be taken for the extension if no branches match.
 */
export interface ExtensionRule {
    readonly extensionId: string;
    readonly branches: readonly ExtensionRuleBranch[];
    readonly defaultAction: ExtensionAction;
}



