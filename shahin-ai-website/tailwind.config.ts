import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0B1F3B',
          light: '#1A3A5C',
          dark: '#05101F',
        },
        accent: {
          DEFAULT: '#0E7490',
          light: '#14B8A6',
          dark: '#0A5D73',
        },
        neutral: {
          gray: '#E5E7EB',
          slate: '#1F2937',
        },
        status: {
          success: '#15803D',
          warning: '#CA8A04',
          danger: '#B91C1C',
          info: '#2563EB',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        arabic: ['Cairo', 'Tajawal', 'Arial', 'sans-serif'],
      },
    },
  },
  plugins: [],
};

export default config;
