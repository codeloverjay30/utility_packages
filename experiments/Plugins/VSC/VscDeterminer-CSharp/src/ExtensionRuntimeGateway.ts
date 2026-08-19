import { ExtensionAction } from "./ExtensionAction";
import { ExtensionActionResult } from "./ExtensionActionResult";
import { ExtensionActionScope } from "./ExtensionActionScope";
import { IExtensionRuntimeGateway } from "./IExtensionRuntimeGateway";
import * as vscode from 'vscode';
import { Message } from "./Message";

export class ExtensionRuntimeGateway implements IExtensionRuntimeGateway {
    async requestInstall(extensionId: string): Promise<ExtensionActionResult> {
        const answer =
            await vscode.window.showWarningMessage(
                `VscDeterminer requests installation of '${extensionId}'.`,
                {
                    modal: true
                },
                Message.Install,
                Message.Cancel
            );

        if (answer !== Message.Install) {
            return {
                extensionId,
                action:
                    ExtensionAction.Install,
                status:
                    ExtensionActionStatus.Declined,
                message:
                    'User declined installation.'
            };
        }

        await vscode.commands.executeCommand(
            'workbench.extensions.installExtension',
            extensionId
        );

        return {
            extensionId,
            action:
                ExtensionAction.Install,
            status:
                ExtensionActionStatus.Applied,
            message:
                'Installation command completed.'
        };
    }
    requestEnable(extensionId: string, scope: ExtensionActionScope): Promise<ExtensionActionResult> {
        throw new Error("Method not implemented.");
    }
    requestDisable(extensionId: string, scope: ExtensionActionScope): Promise<ExtensionActionResult> {
        throw new Error("Method not implemented.");
    }
    /**
     * Determines whether the specified extension is installed in the current VS Code environment.
     *
     * @param extensionId - The unique identifier of the extension to check, e.g.,
     * "ms-python.python"
     * "eamodio.gitlens"
     *
     * @returns
     * true  -> Extension is installed
     * false -> Extension is not installed yet
     */
    public async isInstalled(
        extensionId: string
    ): Promise<boolean> {

        // vscode.extensions.getExtension() 會嘗試從目前 VS Code
        // Extension Host 中取得指定 Extension。
        //
        // 若 Extension 已安裝：
        //     回傳 Extension<T> 物件
        //
        // 若 Extension 未安裝：
        //     回傳 undefined
        const extension = vscode.extensions.getExtension(extensionId);

        return extension !== undefined;
    }
    
}
