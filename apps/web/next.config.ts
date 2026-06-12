// 파일 용도: Next.js 애플리케이션의 빌드 및 라우팅 설정을 정의한다.
// 파일 목적: 공개용 프론트엔드가 typed route와 안정적인 기본 설정으로 동작하게 한다.
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  typedRoutes: true
};

export default nextConfig;
