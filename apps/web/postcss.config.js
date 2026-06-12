// 파일 용도: Tailwind CSS와 Autoprefixer를 PostCSS 파이프라인에 연결한다.
// 파일 목적: Next.js 빌드에서 일관된 CSS 변환을 수행하게 한다.
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {}
  }
};
