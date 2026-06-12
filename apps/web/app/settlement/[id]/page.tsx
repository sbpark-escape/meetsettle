// 파일 용도: 특정 모임의 정산 결과와 송금 목록 화면을 구성한다.
// 파일 목적: 누가 누구에게 얼마를 보내야 하는지 바로 확인하게 한다.
import { getSettlement } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function SettlementPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const settlement = await getSettlement(id);

  if (!settlement) {
    return (
      <main className="mx-auto w-full max-w-4xl px-4 py-10">
        <h1 className="text-3xl font-black text-ink">Settlement not found</h1>
      </main>
    );
  }

  return (
    <main className="mx-auto w-full max-w-5xl px-4 py-10">
      <div className="mb-8">
        <p className="mb-2 text-sm font-bold uppercase tracking-[0.12em] text-mint">{settlement.currency}</p>
        <h1 className="text-4xl font-black text-ink">Transfers</h1>
      </div>
      <div className="grid gap-5 lg:grid-cols-[1fr_380px]">
        <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft">
          <div className="space-y-3">
            {settlement.transfers.map((transfer) => (
              <div className="grid gap-2 rounded-md border border-slate-200 p-3 sm:grid-cols-[1fr_auto_1fr_auto] sm:items-center" key={`${transfer.fromParticipantId}-${transfer.toParticipantId}-${transfer.amount}`}>
                <span className="font-bold text-slate-800">{transfer.fromParticipantName}</span>
                <span className="rounded-md bg-coral/10 px-3 py-1 text-center text-sm font-bold text-coral">{transfer.amount.toLocaleString()}</span>
                <span className="font-bold text-slate-800 sm:text-right">{transfer.toParticipantName}</span>
                <span className="text-sm font-bold text-mint">{transfer.isCompleted ? "Done" : "Open"}</span>
              </div>
            ))}
            {settlement.transfers.length === 0 ? <p className="text-slate-600">No transfers needed.</p> : null}
          </div>
        </section>

        <aside className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft">
          <h2 className="mb-4 text-xl font-black text-ink">Balances</h2>
          <div className="space-y-2">
            {settlement.balances.map((balance) => (
              <div className="rounded-md border border-slate-200 px-3 py-3" key={balance.participantId}>
                <div className="flex items-center justify-between gap-3">
                  <span className="font-bold text-slate-800">{balance.participantName}</span>
                  <span className="font-black text-ink">{balance.netAmount.toLocaleString()}</span>
                </div>
              </div>
            ))}
          </div>
        </aside>
      </div>
    </main>
  );
}
