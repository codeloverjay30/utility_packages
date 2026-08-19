import { expect } from 'chai';
import { fs, vol } from 'memfs';
import {
    FileSystem,
    VscExtensionDeterminerService
} from '../src/vscDeterminer';

/**
 * Unit test suite for VscExtensionDeterminerService using Mocha and memfs.
 */
describe('VscExtensionDeterminerService Multi-Level FileSystem Tests', () => {
    let determinerService: VscExtensionDeterminerService;

    beforeEach(() => {
        vol.reset();

        const memoryFileSystem: FileSystem = {
            existsSync: (filePath: string): boolean =>
                fs.existsSync(filePath),

            readTextFileSync: (filePath: string): string => {
                const content =
                    fs.readFileSync(
                        filePath,
                        'utf-8'
                    );

                return typeof content === 'string'
                    ? content
                    : content.toString('utf-8');
            }
        };

        determinerService =
            new VscExtensionDeterminerService(
                memoryFileSystem
            );
    });

    afterEach(() => {
        vol.reset();
    });

    /**
     * Tests that upward configuration search correctly identifies the nearest
     * target file across a multi-level directory structure.
     */
    it('findConfigFileUpward_AcrossMultiLevelDirectories_ShouldLocateCorrectExtensionsJson', () => {
        // Arrange
        const virtualFiles = {
            '/workspace/examples/ex4/ex4-3/.vscode/extensions.json': JSON.stringify({
                'ms-vscode.PowerShell': {
                    conditions: {
                        Install: {
                            on: ['vsc-workspace-opened']
                        }
                    },
                    defaultsBehavior: 'Install'
                }
            }),
            '/workspace/examples/ex4/ex4-3/ex4-3-1/.vscode/extensions.json': JSON.stringify({
                'dbaeumer.vscode-eslint': {
                    conditions: {
                        Enable: {
                            on: ['vsc-workspace-opened']
                        }
                    },
                    defaultsBehavior: 'Enable'
                }
            })
        };

        vol.fromJSON(virtualFiles);

        const startDir =
            '/workspace/examples/ex4/ex4-3/ex4-3-1';

        // Act
        const foundConfigPath =
            determinerService.findConfigFileUpward(startDir);

        // Assert
        expect(foundConfigPath).to.not.be.null;

        expect(foundConfigPath).to.equal(
            '/workspace/examples/ex4/ex4-3/ex4-3-1/.vscode/extensions.json'
        );
    });

    /**
     * Tests that evaluateAndDetermine throws an explicit defensive error
     * when the configuration file path is empty.
     */
    it('evaluateAndDetermine_WithEmptyPath_ShouldThrowDefensiveException', () => {
        // Act
        const act = () =>
            determinerService.evaluateAndDetermine(
                '',
                'vsc-workspace-opened'
            );

        // Assert
        expect(act)
            .to.throw(
                Error,
                'Configuration file path cannot be empty.'
            );
    });
});