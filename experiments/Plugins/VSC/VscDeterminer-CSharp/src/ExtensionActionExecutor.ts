import { ExtensionAction } from "./ExtensionAction";
import { ExtensionActionResult } from "./ExtensionActionResult";
import { ExtensionDecision } from "./ExtensionDecision";
import { IExtensionActionExecutor } from "./IExtensionActionExecutor";
import { IExtensionRuntimeGateway } from "./IExtensionRuntimeGateway";

/**
    {@inheritdoc}
 */
export class ExtensionActionExecutor implements IExtensionActionExecutor {

    public constructor(
        private readonly runtime:
            IExtensionRuntimeGateway
    ) {}

    public async execute(
        decision: ExtensionDecision
    ): Promise<ExtensionActionResult> {

        const installed =
            await this.runtime.isInstalled(
                decision.extensionId
            );

        if (
            !installed &&
            decision.installWhenMissingOrCorrupted
        ) {
            return this.runtime.requestInstall(
                decision.extensionId
            );
        }

        switch (decision.action) {

            case ExtensionAction.Install:

                if (installed) {
                    return this.skipped(
                        decision,
                        'Extension is already installed.'
                    );
                }

                return this.runtime.requestInstall(
                    decision.extensionId
                );

            case ExtensionAction.Enable:

                if (!installed) {
                    return this.skipped(
                        decision,
                        'Cannot enable an extension that is not installed.'
                    );
                }

                return this.runtime.requestEnable(
                    decision.extensionId,
                    decision.scope
                );

            case ExtensionAction.Disable:

                if (!installed) {
                    return this.skipped(
                        decision,
                        'Extension is not installed, so disabling is unnecessary.'
                    );
                }

                return this.runtime.requestDisable(
                    decision.extensionId,
                    decision.scope
                );

            case ExtensionAction.Ignore:

                return this.skipped(
                    decision,
                    'Ignored explicitly by VscDeterminer rule.'
                );

            case ExtensionAction.Default:
            default:

                return this.skipped(
                    decision,
                    'Delegated to VS Code/default behavior.'
                );
        }
    }

    private skipped(
        decision: ExtensionDecision,
        message: string
    ): ExtensionActionResult {
        return {
            extensionId: decision.extensionId,
            action: decision.action,
            status: ExtensionActionStatus.Skipped,
            message
        };
    }
    
}