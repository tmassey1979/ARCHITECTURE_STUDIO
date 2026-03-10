import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const repoRoot = process.cwd();

function readWorkflow(relativePath) {
  return fs.readFileSync(path.join(repoRoot, relativePath), 'utf8');
}

test('pages workflow skips GitHub Pages configuration for pull requests', () => {
  const workflow = readWorkflow('.github/workflows/pages.yml');

  assert.match(
    workflow,
    /name: Configure GitHub Pages[\s\S]*?if: github\.event_name != 'pull_request'/,
    'Expected configure-pages to be skipped on pull requests.'
  );
  assert.match(
    workflow,
    /name: Upload Pages artifact[\s\S]*?if: steps\.pages-state\.outputs\.ready == 'true'/,
    'Expected Pages artifact upload to depend on Pages readiness.'
  );
});

test('extension packaging workflows compute VSIX names without shell template literals', () => {
  const extensionCi = readWorkflow('.github/workflows/extension-ci.yml');
  const releaseWorkflow = readWorkflow('.github/workflows/release-extension.yml');

  for (const [name, workflow] of [
    ['extension-ci', extensionCi],
    ['release-extension', releaseWorkflow]
  ]) {
    assert.match(
      workflow,
      /VSIX_NAME=\$\(node -p "const p=require\('\.\/package\.json'\);/,
      `Expected ${name} to compute the VSIX name with node -p.`
    );
    assert.doesNotMatch(
      workflow,
      /process\.stdout\.write\(`\$\{/,
      `Expected ${name} to avoid shell-expanded template literals in the packaging command.`
    );
  }
});
