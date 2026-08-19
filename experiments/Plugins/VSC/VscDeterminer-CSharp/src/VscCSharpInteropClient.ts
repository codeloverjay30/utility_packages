import * as cp from 'child_process';
import * as vscode from 'vscode';

/**
 * Represents a communication bridge client to interact with the C# .NET backend utility services.
 */
export class VscCSharpInteropClient {
    private readonly dotnetExecutablePath: string;
    private readonly backendDllPath: string;

    /**
     * Initializes a new instance of the <see cref="VscCSharpInteropClient"/> class.
     * @param dotnetExecutablePath Path to the dotnet CLI runtime.
     * @param backendDllPath Path to the compiled C# backend utility DLL.
     */
    constructor(dotnetExecutablePath: string, backendDllPath: string) {
        if (!dotnetExecutablePath || !backendDllPath) {
            throw new Error("Dotnet executable and backend DLL paths must be provided.");
        }
        this.dotnetExecutablePath = dotnetExecutablePath;
        this.backendDllPath = backendDllPath;
    }

    /**
     * Invokes the C# backend service asynchronously via standard input/output interop.
     * @param command The command to execute in the C# backend.
     * @param args Arguments to pass to the backend.
     * @param cancellationToken Optional VSC cancellation token.
     * @returns A promise containing the standard output string from the C# backend.
     */
    public async invokeBackendServiceAsync(
        command: string,
        args: string[],
        cancellationToken?: vscode.CancellationToken
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            const child = cp.spawn(this.dotnetExecutablePath, [this.backendDllPath, command, ...args], {
                stdio: ['pipe', 'pipe', 'pipe']
            });

            let stdoutData = '';
            let stderrData = '';

            if (cancellationToken) {
                cancellationToken.onCancellationRequested(() => {
                    child.kill();
                    reject(new Error("Operation cancelled by user."));
                });
            }

            child.stdout.on('data', (data: Buffer) => {
                stdoutData += data.toString('utf-8');
            });

            child.stderr.on('data', (data: Buffer) => {
                stderrData += data.toString('utf-8');
            });

            child.on('error', (err) => {
                reject(new Error(`Failed to start C# backend process: ${err.message}`));
            });

            child.on('close', (code) => {
                if (code === 0) {
                    resolve(stdoutData.trim());
                } else {
                    reject(new Error(`C# backend exited with code ${code}. Details: ${stderrData.trim()}`));
                }
            });
        });
    }

    /**
     * Triggers a VSC UI notification using information retrieved from the C# backend.
     * @param message The message retrieved from C#.
     */
    public async showInfoFromBackendAsync(message: string): Promise<void> {
        await vscode.window.showInformationMessage(`[C# Backend]: ${message}`);
    }
}