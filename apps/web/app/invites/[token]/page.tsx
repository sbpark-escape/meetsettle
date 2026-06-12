// 파일 용도: 초대 토큰으로 접근한 사용자의 참가 입력 화면을 구성한다.
// 파일 목적: 로그인 없는 MVP 초대 흐름의 프론트엔드 확장 지점을 제공한다.
export const dynamic = "force-dynamic";

export default async function InvitePage({ params }: { params: Promise<{ token: string }> }) {
  const { token } = await params;

  return (
    <main className="mx-auto w-full max-w-4xl px-4 py-10">
      <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-soft">
        <p className="mb-2 text-sm font-bold uppercase tracking-[0.12em] text-mint">Invite</p>
        <h1 className="text-3xl font-black text-ink">Join meetup</h1>
        <p className="mt-3 break-all text-slate-600">{token}</p>
        <div className="mt-6 grid gap-3 sm:grid-cols-[1fr_auto]">
          <input className="rounded-md border border-slate-300 px-3 py-3 outline-none focus:border-mint" placeholder="Your name" />
          <button className="rounded-md bg-ink px-4 py-3 font-bold text-white">Join</button>
        </div>
      </section>
    </main>
  );
}
