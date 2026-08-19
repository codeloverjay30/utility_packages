import {
    ExtensionActionResult,
} from './ExtensionActionResult';
import { ExtensionActionScope } from './ExtensionActionScope';

/**
    * Interface for interacting with the extension runtime environment.
    @param extensionId - The unique identifier of the extension.
    @returns A promise that resolves to a boolean indicating whether the extension is installed.
    @returns A promise that resolves to an ExtensionActionResult indicating the success or failure of the action request.
 */
export interface IExtensionRuntimeGateway {
    isInstalled(
        extensionId: string
    ): Promise<boolean>;

    requestInstall(
        extensionId: string
    ): Promise<ExtensionActionResult>;

    requestEnable(
        extensionId: string,
        scope: ExtensionActionScope
    ): Promise<ExtensionActionResult>;

    requestDisable(
        extensionId: string,
        scope: ExtensionActionScope
    ): Promise<ExtensionActionResult>;
}