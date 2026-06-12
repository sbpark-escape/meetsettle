// 파일 용도: MeetSettle 웹 앱의 공통 HTML 레이아웃과 상단 내비게이션을 정의한다.
// 파일 목적: 모든 화면에서 동일한 메타데이터와 기본 이동 경로를 제공한다.
import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "MeetSettle",
  description: "Open-source meetup attendance and settlement toolkit",
  manifest: "/manifest.json"
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <header className="border-b border-slate-200/80 bg-white/88 backdrop-blur">
          <nav className="mx-auto flex min-h-16 w-full max-w-6xl items-center justify-between px-4">
            <Link className="text-lg font-bold text-ink" href="/">
              MeetSettle
            </Link>
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-600">
              <Link className="rounded-md px-3 py-2 hover:bg-slate-100" href="/meetups/new">
                Create
              </Link>
              <Link className="rounded-md px-3 py-2 hover:bg-slate-100" href="/docs">
                Docs
              </Link>
            </div>
          </nav>
        </header>
        {children}
      </body>
    </html>
  );
}
