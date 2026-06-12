// 파일 용도: MeetSettle 웹 UI의 Tailwind CSS 스캔 경로와 디자인 토큰을 정의한다.
// 파일 목적: 공개용 예제 앱이 가볍고 일관된 모바일 우선 스타일을 갖게 한다.
import type { Config } from "tailwindcss";

const config: Config = {
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}", "./lib/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: "#18212f",
        mist: "#f5f7fb",
        coral: "#e45d50",
        mint: "#2c9f7a",
        gold: "#d19a2a"
      },
      boxShadow: {
        soft: "0 18px 45px rgba(24, 33, 47, 0.10)"
      }
    }
  },
  plugins: []
};

export default config;
