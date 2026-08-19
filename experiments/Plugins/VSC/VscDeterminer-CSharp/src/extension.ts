import * as vscode from 'vscode';
import * as path from 'path';
import * as os from 'os';
import * as fs from 'fs';
import { VscExtensionDeterminerService } from './vscDeterminer';

/**
 * Activates the VscDeterminer extension when the workspace is opened.
 * @param context The extension context provided by VS Code.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    console.log("========== VscDeterminer DEBUG ==========");

    console.log(
        "workspaceFolders =",
        vscode.workspace.workspaceFolders?.map(
        folder => folder.uri.fsPath
        )
    );
    

    console.log(
        "workspaceFile =",
        vscode.workspace.workspaceFile?.fsPath
    );
    

    console.log(
        "cwd =",
        process.cwd()
    );
    

    console.log(
        "extensionPath =",
        context.extensionPath
    );
    

    console.log(
        "extensionUri =",
        context.extensionUri.fsPath
    );
    
    console.log("=========================================");

    // Register command to open the Workspace Settings Manager panel
    const openManagerDisposable = vscode.commands.registerCommand('vscDeterminer.openSettingsManager', () => {
        WorkspaceSettingsManagerPanel.createOrShow(context.extensionUri);
    });

    // Register command for context menu item "View Workspace Settings" (Linked to tree view title right-click)
    const viewSettingsDisposable = vscode.commands.registerCommand('vscDeterminer.viewWorkspaceSettings', async () => {
        await WorkspaceSettingsManagerPanel.renderSettingsTabAsync();
    });

    context.subscriptions.push(openManagerDisposable, viewSettingsDisposable);

    try {
        const workspaceFolders = vscode.workspace.workspaceFolders;
        if (!workspaceFolders || workspaceFolders.length === 0) {
            console.warn("VscDeterminer: No active workspace folder found.");
            return;
        }

        // Register the TreeDataProvider strictly matching the view ID in package.json
        const treeDataProvider = new WorkspaceSettingsTreeDataProvider();
        const treeView = vscode.window.createTreeView('vscSettingsExplorer', {
            treeDataProvider: treeDataProvider,
            showCollapseAll: true
        });
        
        context.subscriptions.push(treeView);

        const rootPath = workspaceFolders[0].uri.fsPath;
        
        // Use MSBuild-style upward search instead of fixed path joining
        const determinerService = new VscExtensionDeterminerService();
        const configFilePath = determinerService.findConfigFileUpward(rootPath);

        if (configFilePath && fs.existsSync(configFilePath))
        {
            const actions = determinerService.evaluateAndDetermine(configFilePath, 'vsc-workspace-opened');
            for (const [extensionId, action] of actions)
            {
                if (action === 'Install')
                {
                    try {
                        // 實際執行 VS Code 擴充功能安裝命令
                        await vscode.commands.executeCommand('workbench.extensions.installExtension', extensionId);
                        console.log(`Successfully auto-installed extension: ${extensionId}`);
                    } catch (error: unknown) {
                        const errMessage = error instanceof Error ? error.message : String(error);
                        console.error(`Failed to auto-install extension ${extensionId}: ${errMessage}`);
                    }
                }
            }
        }
    } catch (error: unknown) {
        const errorMessage = error instanceof Error ? error.message : String(error);
        vscode.window.showErrorMessage(`VscDeterminer Initialization Error: ${errorMessage}`);
    }
}

/**
 * Provides data representation for the Workspace Settings Explorer TreeView.
 */
class WorkspaceSettingsTreeDataProvider implements vscode.TreeDataProvider<WorkspaceSettingsTreeItem> {
    private _onDidChangeTreeData: vscode.EventEmitter<WorkspaceSettingsTreeItem | undefined | void> = new vscode.EventEmitter<WorkspaceSettingsTreeItem | undefined | void>();
    readonly onDidChangeTreeData: vscode.Event<WorkspaceSettingsTreeItem | undefined | void> = this._onDidChangeTreeData.event;

    constructor() {}

    /**
     * Refreshes the tree view data provider.
     */
    public refresh(): void {
        this._onDidChangeTreeData.fire();
    }

    /**
     * Gets the tree item representation.
     * @param element The tree item node.
     */
    getTreeItem(element: WorkspaceSettingsTreeItem): vscode.TreeItem {
        return element;
    }

    /**
     * Retrieves children nodes for the tree view.
     * @param element The parent element.
     */
    getChildren(element?: WorkspaceSettingsTreeItem): Thenable<WorkspaceSettingsTreeItem[]> {
        try {
            if (!element) {
                return Promise.resolve([
                    new WorkspaceSettingsTreeItem('Workspace Settings Status', vscode.TreeItemCollapsibleState.None, {
                        command: 'vscDeterminer.viewWorkspaceSettings',
                        title: 'View Workspace Settings'
                    })
                ]);
            }
            return Promise.resolve([]);
        } catch (error: unknown) {
            const err = error as Error;
            vscode.window.showErrorMessage(`Failed to load tree view data: ${err.message}`);
            return Promise.resolve([]);
        }
    }
}

/**
 * Represents an individual item within the Workspace Settings Explorer.
 */
class WorkspaceSettingsTreeItem extends vscode.TreeItem {
    constructor(
        public readonly label: string,
        public readonly collapsibleState: vscode.TreeItemCollapsibleState,
        public readonly commandToExecute?: vscode.Command
    ) {
        super(label, collapsibleState);
        this.tooltip = `${this.label} - Click to view details`;
        this.description = 'Ready';
        if (commandToExecute) {
            this.command = commandToExecute;
        }
    }
}

/**
 * Manages the Workspace Settings Manager Webview Panel.
 */
class WorkspaceSettingsManagerPanel {
    public static currentPanel: WorkspaceSettingsManagerPanel | undefined;
    private readonly _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];

    /**
     * Initializes a new instance of the WorkspaceSettingsManagerPanel class.
     * @param panel The webview panel.
     * @param extensionUri The extension URI.
     */
    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }

    /**
     * Creates or displays the settings manager panel.
     * @param extensionUri The extension URI.
     */
    public static createOrShow(extensionUri: vscode.Uri): void {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;

        if (WorkspaceSettingsManagerPanel.currentPanel) {
            WorkspaceSettingsManagerPanel.currentPanel._panel.reveal(column);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'vscSettingsManager',
            'Workspace Settings Manager',
            column || vscode.ViewColumn.One,
            { enableScripts: true }
        );

        WorkspaceSettingsManagerPanel.currentPanel = new WorkspaceSettingsManagerPanel(panel, extensionUri);
    }

    /**
     * Atomically renders the settings tab analyzing global/local settings and extension states.
     */
    public static async renderSettingsTabAsync(): Promise<void> {
        WorkspaceSettingsManagerPanel.createOrShow(vscode.Uri.file(__dirname));
        const panelInstance = WorkspaceSettingsManagerPanel.currentPanel;
        if (!panelInstance) {
            return;
        }

        // 1. Determine device global settings path (e.g., %APPDATA%\Code\User\settings.json)
        const appDataPath = process.env.APPDATA || (process.platform === 'darwin' ? path.join(os.homedir(), 'Library', 'Application Support') : path.join(os.homedir(), '.config'));
        const globalSettingsPath = path.join(appDataPath, 'Code', 'User', 'settings.json');
        
        // 2. Determine workspace local settings path
        const workspaceFolders = vscode.workspace.workspaceFolders;
        let localWorkspaceSettingsPath = 'N/A (No active workspace)';
        if (workspaceFolders && workspaceFolders.length > 0) {
            localWorkspaceSettingsPath = path.join(workspaceFolders[0].uri.fsPath, '.vscode', 'settings.json');
        }

        // 3. Evaluate extension automatic installation, enabling, and disabling actions based on project requirements
        const determinerService = new VscExtensionDeterminerService();
        let evaluatedActions: Map<string, string> = new Map();
        if (workspaceFolders && workspaceFolders.length > 0) {
            const configPath = path.join(workspaceFolders[0].uri.fsPath, '.vscode', 'extensions.json');
            if (fs.existsSync(configPath)) {
                evaluatedActions = determinerService.evaluateAndDetermine(configPath, 'vsc-workspace-opened');
            }
        }

        let extensionHtmlRows = '';
        evaluatedActions.forEach((action, extId) => {
            extensionHtmlRows += `<tr><td>${extId}</td><td>${action}</td></tr>`;
        });

        if (!extensionHtmlRows) {
            extensionHtmlRows = `<tr><td colspan="2">No specific extension actions evaluated.</td></tr>`;
        }

        // 4. Populate Webview HTML markup
        panelInstance._panel.webview.html = `
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>Workspace Settings Manager</title>
                <style>
                    body { font-family: var(--vscode-font-family); color: var(--vscode-editor-foreground); padding: 20px; background-color: var(--vscode-editor-background); }
                    table { width: 100%; border-collapse: collapse; margin-top: 15px; }
                    th, td { border: 1px solid var(--vscode-panel-border); padding: 8px 12px; text-align: left; }
                    th { background-color: var(--vscode-list-hoverBackground); }
                </style>
            </head>
            <body>
                <h2>Workspace Configuration & Extension Manager Analysis</h2>
                <p><strong>Global Device Settings Path:</strong> ${globalSettingsPath}</p>
                <p><strong>Active Workspace Settings Path:</strong> ${localWorkspaceSettingsPath}</p>
                
                <h3>Evaluated VSC Extension Lifecycle Actions</h3>
                <table>
                    <thead>
                        <tr><th>Extension Identifier</th><th>Determined Action (Auto-Install / Enable / Disable)</th></tr>
                    </thead>
                    <tbody>
                        ${extensionHtmlRows}
                    </tbody>
                </table>
            </body>
            </html>
        `;
        
        vscode.window.showInformationMessage("Workspace settings and plugin behaviors successfully loaded.");
    }

    /**
     * Disposes the panel resources cleanly.
     */
    public dispose(): void {
        WorkspaceSettingsManagerPanel.currentPanel = undefined;
        this._panel.dispose();
        while (this._disposables.length) {
            const disposable = this._disposables.pop();
            if (disposable) {
                disposable.dispose();
            }
        }
    }
}

/**
 * Deactivates the VscDeterminer extension.
 */
export function deactivate(): void {}