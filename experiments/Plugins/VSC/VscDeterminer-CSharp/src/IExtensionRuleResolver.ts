import { ExtensionDecision } from "./ExtensionDecision";
import { ExtensionRule, VscEnvironmentEvent } from "./ExtensionRule";

/**
    * Interface for resolving extension rules based on the current environment event.
    @param rule - The extension rule to resolve.
    @param currentEvent - The current environment event.
    @returns The resolved extension decision.
 */
export interface IExtensionRuleResolver {

    resolve(
        rule: ExtensionRule,
        currentEvent: VscEnvironmentEvent
    ): ExtensionDecision;
}