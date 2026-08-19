import { ExtensionActionResult } from './ExtensionActionResult';
import { ExtensionDecision } from './ExtensionDecision';

/**
    * Interface for executing extension actions.
    @param extensionId - The unique identifier of the extension.
    @param action - The action to be executed (Install, Enable, Disable, Ignore, Default).
    @param scope - The scope in which the action should be executed (Global or Workspace).
    @returns A promise that resolves to an ExtensionActionResult indicating the success or failure of the action execution.
 */
export interface IExtensionActionExecutor {
    execute(
        decision: ExtensionDecision
    ): Promise<ExtensionActionResult>;
}