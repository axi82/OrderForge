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
          DEFAULT: '#1b6ec2',
          dark: '#1861ac',
          ring: '#258cfb',
        },
        link: '#0071c1',
      },
    },
  },
  plugins: [],
};
