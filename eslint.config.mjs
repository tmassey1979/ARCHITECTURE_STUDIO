import js from "@eslint/js";
import tseslint from "@typescript-eslint/eslint-plugin";
import tsParser from "@typescript-eslint/parser";
import globals from "globals";

export default [
  {
    ignores: ["out/**", "node_modules/**", "coverage/**", "core/**/bin/**", "core/**/obj/**"]
  },
  js.configs.recommended,
  {
    files: ["src/**/*.ts"],
    languageOptions: {
      parser: tsParser,
      parserOptions: {
        project: false,
        ecmaVersion: 2022,
        sourceType: "script"
      },
      globals: {
        ...globals.node
      }
    },
    plugins: {
      "@typescript-eslint": tseslint
    },
    rules: {
      ...tseslint.configs.recommended.rules,
      "@typescript-eslint/consistent-type-imports": "error"
    }
  },
  {
    files: ["test/**/*.mjs", "*.mjs"],
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: "module",
      globals: {
        ...globals.node
      }
    }
  }
];
