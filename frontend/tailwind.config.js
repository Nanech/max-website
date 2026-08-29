export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    fontFamily: {
      mono: ['Menlo', 'monospace'],
      //   sans: ['Inter', 'sans-serif'],
    },
    extend: {
      colors: {
        'primary-white': '#FFFFFF',
        'primary-black': '#111111',
        'primary-gray': '#7E7E7E',
      },
    },
  },
  plugins: [],
}
