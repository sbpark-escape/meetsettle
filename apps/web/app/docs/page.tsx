// 파일 용도: 공개 프로젝트의 앱 내 문서 요약 화면을 구성한다.
// 파일 목적: API, core, PWA 확장 방향을 웹 화면에서도 빠르게 확인하게 한다.
export default function DocsPage() {
  return (
    <main className="mx-auto w-full max-w-4xl px-4 py-10">
      <h1 className="text-4xl font-black text-ink">Docs</h1>
      <div className="mt-6 grid gap-4">
        {[
          ["API", "Create meetups, participants, expenses, invite tokens, and settlement transfers."],
          ["Core", "Use the settlement engine without the example app."],
          ["PWA", "Manifest metadata is included. Notifications are not included yet."]
        ].map(([title, text]) => (
          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft" key={title}>
            <h2 className="text-xl font-black text-ink">{title}</h2>
            <p className="mt-2 text-slate-600">{text}</p>
          </section>
        ))}
      </div>
    </main>
  );
}
