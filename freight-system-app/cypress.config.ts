import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:4200',
    supportFile: false,
    setupNodeEvents(on, config) {
      // configure events here if needed
      return config;
    }
  }
});
