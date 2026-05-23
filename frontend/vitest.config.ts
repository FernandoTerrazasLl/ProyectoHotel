import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    globals: true,
    coverage: {
      provider: 'istanbul',
      reporter: ['text', 'html'],
      reportsDirectory: './coverage',
      all: true,
      include: ['src/**/*.{ts,tsx,js,jsx}'],
      exclude: [
        'node_modules/',
        'dist/',
        'vite.config.ts',
        'vitest.config.ts',
        '**/*.d.ts',
        '**/*.test.{ts,tsx,js,jsx}',
        'src/__tests__/**'
      ]
    }
  }
});
