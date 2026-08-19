import * as fs from 'fs';
import * as path from 'path';

/**
 * Interface representing individual condition criteria for an extension rule.
 */
interface ExtensionConditionRule {
    on: string[];
    ifNotInstalledOrCorrupted?: { [key: string]: any };
}

/**
 * Interface representing the configuration rules for a specific VS Code extension.
 */
interface ExtensionRuleConfiguration {
    conditions: { [behavior: string]: ExtensionConditionRule };
    defaultsBehavior?: string;
    allEvents?: { [key: string]: any };
}

/**
 * Defines the minimal file-system operations required by the
 * extension determiner service.
 */
export interface FileSystem {
    /**
     * Determines whether the specified path exists.
     *
     * @param filePath The file-system path to inspect.
     * @returns True when the path exists; otherwise, false.
     */
    existsSync(filePath: string): boolean;

    /**
     * Reads the specified file as UTF-8 text.
     *
     * @param filePath The file-system path to read.
     * @returns The UTF-8 decoded file content.
     */
    readTextFileSync(filePath: string): string;
}

/**
 * Provides the production Node.js file-system implementation.
 */
const nodeFileSystem: FileSystem = {
    existsSync: (filePath: string): boolean =>
        fs.existsSync(filePath),

    readTextFileSync: (filePath: string): string =>
        fs.readFileSync(filePath, 'utf-8')
};

/**
 * Core engine responsible for evaluating multi-conditional VS Code extension
 * rules and locating configuration files using upward-search semantics.
 */
export class VscExtensionDeterminerService {

    private readonly fs: FileSystem;

    /**
     * Initializes a new instance of the VscExtensionDeterminerService class.
     *
     * @param fsImpl The file-system implementation used by the service.
     */
    public constructor(
        fsImpl: FileSystem = nodeFileSystem
    ) {
        if (!fsImpl) {
            throw new Error(
                'File-system implementation cannot be null or undefined.'
            );
        }

        this.fs = fsImpl;
    }

    /**
     * Searches upward from the starting directory for the target configuration
     * file using MSBuild-style nearest-parent precedence.
     *
     * @param startDir The directory from which the upward search starts.
     * @param targetFileName The relative configuration file path to locate.
     * @returns The absolute configuration path when found; otherwise, null.
     */
    public findConfigFileUpward(
        startDir: string,
        targetFileName: string = '.vscode/extensions.json'
    ): string | null {
        if (!startDir || startDir.trim() === '') {
            return null;
        }

        if (!this.fs.existsSync(startDir)) {
            return null;
        }

        const pathApi =
            this.getPathImplementation(startDir);

        let currentDir =
            pathApi.resolve(startDir);

        const parsedRoot =
            pathApi.parse(currentDir).root;

        while (true) {
            const candidatePath =
                pathApi.join(
                    currentDir,
                    targetFileName
                );

            if (this.fs.existsSync(candidatePath)) {
                return candidatePath;
            }

            if (currentDir === parsedRoot) {
                break;
            }

            const parentDir =
                pathApi.dirname(currentDir);

            if (parentDir === currentDir) {
                break;
            }

            currentDir = parentDir;
        }

        return null;
    }

    /**
     * Evaluates the extension configuration file from the specified path and determines actions.
     *
     * @param configFilePath The absolute path to the extension configuration JSON file.
     * @param currentEnvironmentEvent The current trigger event.
     * @returns A dictionary containing the determined action for each extension.
     */
    public evaluateAndDetermine(
        configFilePath: string,
        currentEnvironmentEvent: string
    ): Map<string, string> {
        if (
            !configFilePath ||
            configFilePath.trim() === ''
        ) {
            throw new Error(
                'Configuration file path cannot be empty.'
            );
        }

        if (!this.fs.existsSync(configFilePath)) {
            throw new Error(
                `The configuration file was not found at: ${configFilePath}`
            );
        }

        try {
            const rawData =
                this.fs.readTextFileSync(
                    configFilePath
                );

            const rules: {
                [extensionName: string]:
                    ExtensionRuleConfiguration;
            } = JSON.parse(rawData);

            const results =
                new Map<string, string>();

            for (
                const [extensionName, config]
                of Object.entries(rules)
            ) {
                const determinedAction =
                    this.resolveAction(
                        config,
                        currentEnvironmentEvent
                    );

                results.set(
                    extensionName,
                    determinedAction
                );
            }

            return results;
        } catch (error: unknown) {
            const errorMessage =
                error instanceof Error
                    ? error.message
                    : String(error);

            throw new Error(
                `Failed to parse extension configuration file: ${errorMessage}`
            );
        }
    }

    /**
     * Resolves the specific action for an extension based on its multi-level conditions.
     *
     * @param config The extension rule configuration.
     * @param currentEvent The current execution event.
     * @returns The resolved action string.
     */
    private resolveAction(
        config: ExtensionRuleConfiguration,
        currentEvent: string
    ): string {
        if (config.conditions) {
            for (
                const [behaviorKey, rule]
                of Object.entries(config.conditions)
            ) {
                if (
                    rule.on &&
                    rule.on.includes(currentEvent)
                ) {
                    return behaviorKey;
                }
            }
        }

        return config.defaultsBehavior
            || 'defaultDetermined';
    }

    /**
     * Selects deterministic path semantics based on the supplied path.
     *
     * @param inputPath The path whose syntax should be inspected.
     * @returns The appropriate Windows or POSIX path implementation.
     */
    private getPathImplementation(
        inputPath: string
    ): typeof path.posix | typeof path.win32 {
        const hasWindowsDrivePrefix =
            /^[A-Za-z]:[\\/]/.test(inputPath);

        const isWindowsUncPath =
            inputPath.startsWith('\\\\');

        if (
            hasWindowsDrivePrefix ||
            isWindowsUncPath
        ) {
            return path.win32;
        }

        if (inputPath.startsWith('/')) {
            return path.posix;
        }

        return process.platform === 'win32'
            ? path.win32
            : path.posix;
    }
}