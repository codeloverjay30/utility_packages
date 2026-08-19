"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const assert = require("assert");
const fs = require("fs");
const path = require("path");
const os = require("os");
const vscDeterminer_1 = require("../src/vscDeterminer");
/**
 * Unit test suite for VscExtensionDeterminerService.
 */
describe('VscExtensionDeterminerService Tests', () => {
    let tempDir;
    let configFilePath;
    let determinerService;
    beforeEach(() => {
        // Create a temporary directory and file for testing isolation
        tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'vsc-determiner-'));
        configFilePath = path.join(tempDir, 'extensions.json');
        determinerService = new vscDeterminer_1.VscExtensionDeterminerService();
    });
    afterEach(() => {
        // Cleanup temporary files and directory
        if (fs.existsSync(configFilePath)) {
            fs.unlinkSync(configFilePath);
        }
        if (fs.existsSync(tempDir)) {
            fs.rmdirSync(tempDir);
        }
    });
    /**
     * Tests that evaluateAndDetermine throws an error when config file path is empty.
     */
    it('evaluateAndDetermine_WithEmptyPath_ShouldThrowError', () => {
        // Act & Assert
        assert.throws(() => {
            determinerService.evaluateAndDetermine('', 'vsc-workspace-opened');
        }, /Configuration file path cannot be empty/);
    });
    /**
     * Tests that evaluateAndDetermine throws an error when the configuration file does not exist.
     */
    it('evaluateAndDetermine_WithNonExistentFile_ShouldThrowError', () => {
        // Arrange
        const nonExistentPath = path.join(tempDir, 'non-existent.json');
        // Act & Assert
        assert.throws(() => {
            determinerService.evaluateAndDetermine(nonExistentPath, 'vsc-workspace-opened');
        }, /The configuration file was not found/);
    });
    /**
     * Tests that evaluateAndDetermine successfully resolves actions based on matching condition rules.
     */
    it('evaluateAndDetermine_WithValidConfigAndMatchingEvent_ShouldResolveAction', () => {
        // Arrange
        const sampleConfig = {
            "publisher.extension-a": {
                "conditions": {
                    "Install": {
                        "on": ["vsc-workspace-opened"]
                    }
                },
                "defaultsBehavior": "defaultDetermined"
            }
        };
        fs.writeFileSync(configFilePath, JSON.stringify(sampleConfig), 'utf-8');
        // Act
        const results = determinerService.evaluateAndDetermine(configFilePath, 'vsc-workspace-opened');
        // Assert
        assert.strictEqual(results.size, 1);
        assert.strictEqual(results.get("publisher.extension-a"), "Install");
    });
    /**
     * Tests that evaluateAndDetermine falls back to defaultsBehavior when events do not match.
     */
    it('evaluateAndDetermine_WithNonMatchingEvent_ShouldFallbackToDefaults', () => {
        // Arrange
        const sampleConfig = {
            "publisher.extension-b": {
                "conditions": {
                    "Install": {
                        "on": ["vsc-opened"]
                    }
                },
                "defaultsBehavior": "Ignore"
            }
        };
        fs.writeFileSync(configFilePath, JSON.stringify(sampleConfig), 'utf-8');
        // Act
        const results = determinerService.evaluateAndDetermine(configFilePath, 'vsc-workspace-opened');
        // Assert
        assert.strictEqual(results.size, 1);
        assert.strictEqual(results.get("publisher.extension-b"), "Ignore");
    });
});
//# sourceMappingURL=vscDeterminer.test.js.map