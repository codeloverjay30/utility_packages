import { ExtensionAction } from "./ExtensionAction";
import { ExtensionActionScope } from "./ExtensionActionScope";
import { ExtensionDecision } from "./ExtensionDecision";
import { ExtensionRule, VscEnvironmentEvent } from "./ExtensionRule";
import { IExtensionRuleResolver } from "./IExtensionRuleResolver";


/**
    {@inheritdoc}
 */
export class ExtensionRuleResolver implements IExtensionRuleResolver {

    public resolve(
        rule: ExtensionRule,
        currentEvent: VscEnvironmentEvent
    ): ExtensionDecision {

        const matched = rule.branches.find(
            branch =>
                branch.on.includes(currentEvent)
        );

        if (matched) {
            return {
                extensionId: rule.extensionId,

                action: matched.action,

                scope: matched.scope,

                installWhenMissingOrCorrupted:
                    matched.installWhenMissingOrCorrupted,

                matchedEvent: currentEvent
            };
        }

        return {
            extensionId: rule.extensionId,

            action:
                rule.defaultAction
                ?? ExtensionAction.Default,

            scope:
                ExtensionActionScope.Workspace,

            installWhenMissingOrCorrupted:
                false
        };
    }
}