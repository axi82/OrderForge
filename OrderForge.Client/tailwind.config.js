/**
 * ForgeClean design tokens:
 * - CTAs / cart actions: accent (teal) — use bg-accent, hover:bg-accent-dark, text-white
 * - Nav chrome, brand links in header: primary (navy) — use bg-primary for header bar
 */
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.{razor,html,cs,cshtml}',
    './wwwroot/**/*.{html,js}',
    './**/*.razor.css',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#1E3A8A',
          dark: '#172554',
          ring: '#2563eb',
        },
        accent: {
          DEFAULT: '#0d9488',
          dark: '#0f766e',
          ring: '#14b8a6',
        },
        link: '#1e40af',
      },
    },
  },
  plugins: [],
};
