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
      "no-unused-vars": "warn",
      "no-undef": "warn",
      "eqeqeq": "warn",
      "curly": "warn",
      "no-console": "off"
    }
  }
];
