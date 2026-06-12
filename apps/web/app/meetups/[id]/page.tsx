// 파일 용도: 특정 모임의 참가자와 비용 현황 화면을 구성한다.
// 파일 목적: API 조회 결과를 모바일에서도 읽기 쉬운 모임 상세 정보로 보여준다.
import Link from "next/link";
import { getMeetup } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function MeetupDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const meetup = await getMeetup(id);

  if (!meetup) {
    return (
      <main className="mx-auto w-full max-w-4xl px-4 py-10">
        <h1 className="text-3xl font-black text-ink">Meetup not found</h1>
      </main>
    );
  }

  return (
    <main className="mx-auto w-full max-w-5xl px-4 py-10">
      <div className="mb-8 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <p className="mb-2 text-sm font-bold uppercase tracking-[0.12em] text-mint">{meetup.currency}</p>
          <h1 className="text-4xl font-black text-ink">{meetup.name}</h1>
          <p className="mt-2 text-slate-600">{meetup.date}{meetup.location ? ` · ${meetup.location}` : ""}</p>
        </div>
        <Link className="rounded-md bg-ink px-4 py-3 text-center text-sm font-bold text-white" href={`/settlement/${meetup.id}`}>
          View settlement
        </Link>
      </div>

      <div className="grid gap-5 lg:grid-cols-2">
        <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft">
          <h2 className="mb-4 text-xl font-black text-ink">Participants</h2>
          <div className="space-y-2">
            {meetup.participants.map((participant) => (
              <div className="flex items-center justify-between rounded-md border border-slate-200 px-3 py-3" key={participant.id}>
                <span className="font-bold text-slate-800">{participant.name}</span>
                <span className="text-sm font-bold text-mint">{participant.isAttending ? "Attending" : "Not attending"}</span>
              </div>
            ))}
            {meetup.participants.length === 0 ? <p className="text-slate-600">No participants yet.</p> : null}
          </div>
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft">
          <h2 className="mb-4 text-xl font-black text-ink">Expenses</h2>
          <div className="space-y-2">
            {meetup.expenses.map((expense) => (
              <div className="rounded-md border border-slate-200 px-3 py-3" key={expense.id}>
                <div className="flex items-center justify-between gap-3">
                  <span className="font-bold text-slate-800">{expense.title}</span>
                  <span className="font-black text-ink">{expense.amount.toLocaleString()}</span>
                </div>
                <p className="mt-1 text-sm text-slate-500">{expense.shares.length} share(s)</p>
              </div>
            ))}
            {meetup.expenses.length === 0 ? <p className="text-slate-600">No expenses yet.</p> : null}
          </div>
        </section>
      </div>
    </main>
  );
}
