import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const repoRoot = process.cwd();

function readJson(relativePath) {
  const fullPath = path.join(repoRoot, relativePath);
  return JSON.parse(fs.readFileSync(fullPath, 'utf8'));
}

test('bootstrap scaffold includes the required workspace layout', () => {
  const requiredPaths = [
    'src',
    'core',
    'analysis',
    'generators',
    'graph',
    'ui/dashboard',
    'ui/webview',
    'standards',
    'compliance/regulations',
    'compliance/controls',
    'templates/projects',
    'templates/pipelines',
    'templates/infra',
    'reports',
    'docs/developer/local-development.md',
    'changelog'
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(
      fs.existsSync(path.join(repoRoot, relativePath)),
      `Expected '${relativePath}' to exist.`
    );
  }
});

test('extension manifest contributes baseline metadata and command activation', () => {
  assert.ok(fs.existsSync(path.join(repoRoot, 'package.json')), 'Expected package.json to exist.');
  assert.ok(fs.existsSync(path.join(repoRoot, 'tsconfig.json')), 'Expected tsconfig.json to exist.');
  assert.ok(fs.existsSync(path.join(repoRoot, 'src/extension.ts')), 'Expected src/extension.ts to exist.');

  const manifest = readJson('package.json');

  assert.equal(manifest.main, './out/extension.js');
  assert.equal(manifest.type, 'commonjs');
  assert.equal(manifest.engines.vscode?.length > 0, true);
  assert.ok(Array.isArray(manifest.activationEvents), 'Expected activationEvents to be an array.');
  assert.ok(Array.isArray(manifest.contributes?.commands), 'Expected contributes.commands to be an array.');
  assert.ok(
    manifest.contributes.commands.some((command) => command.command === 'architectureStudio.openDashboard'),
    'Expected the baseline dashboard command contribution.'
  );
});

test('core C# scaffold exists with a solution and tests', () => {
  const requiredPaths = [
    'core/ArchitectureStudio.sln',
    'core/ArchitectureStudio.Core/ArchitectureStudio.Core.csproj',
    'core/ArchitectureStudio.Core.Tests/ArchitectureStudio.Core.Tests.csproj'
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(
      fs.existsSync(path.join(repoRoot, relativePath)),
      `Expected '${relativePath}' to exist.`
    );
  }
});
