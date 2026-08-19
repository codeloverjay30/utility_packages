import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { exec } from 'child_process';

export function activate(context: vscode.ExtensionContext) {
    console.log('WorkspaceSettingsUtility is now active.');

    // 觸發條件：當 Workspace Folder 改變或插件啟動時
    if (vscode.workspace.workspaceFolders) {
        processWorkspaces(vscode.workspace.workspaceFolders);
    }

    context.subscriptions.push(
        vscode.workspace.onDidChangeWorkspaceFolders(e => {
            processWorkspaces(e.added);
        })
    );
}

async function processWorkspaces(folders: readonly vscode.WorkspaceFolder[]) {
    for (const folder of folders) {
        const workspacePath = folder.uri.fsPath;
        // 向上尋找配置檔邏輯
        const configPath = findConfigUpwards(workspacePath);
        
        if (configPath) {
            // 呼叫 .NET CLI 工具
            triggerDotNetBackend(configPath, workspacePath, "vsc-workspace-onentered");
        }
    }
}

function findConfigUpwards(currentPath: string): string | null {
    let dir = currentPath;
    while (dir !== path.parse(dir).root) {
        const configPath = path.join(dir, '.vscode', 'vsc-workspaces', 'settings.json5');
        if (fs.existsSync(configPath)) return configPath;
        dir = path.dirname(dir);
    }
    return null;
}

function triggerDotNetBackend(configPath: string, targetPath: string, event: string) {
    const backendPath = "path/to/your/dotnet/Utility.dll";
    const command = `dotnet ${backendPath} --config "${configPath}" --target "${targetPath}" --event "${event}"`;
    
    exec(command, (err, stdout, stderr) => {
        if (err) vscode.window.showErrorMessage(`Utility Error: ${stderr}`);
        else console.log(stdout);
    });
}