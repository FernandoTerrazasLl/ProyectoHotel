import js from "@eslint/js";

export default [
  js.configs.recommended,
  {
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        window: "readonly",
        document: "readonly",
        fetch: "readonly",
        HTMLElement: "readonly",
        customElements: "readonly",
        console: "readonly",
        Date: "readonly",
        URL: "readonly",
        Number: "readonly",
        Math: "readonly",
        Array: "readonly",
        setTimeout: "readonly",
        FormData: "readonly",
        URLSearchParams: "readonly",
        history: "readonly",
        global: "readonly"
      }
    },
    rules: {
      "no-unused-vars": "error",
      "no-undef": "error",
      "eqeqeq": "error",
      "curly": "error",
      "no-console": "warn",
      "semi": ["error", "always"],
      "quotes": ["error", "double"],
      "prefer-const": "error",
      "no-var": "error"
    }
  }
];
