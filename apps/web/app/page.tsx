import Link from "next/link";

const sampleTransfers = [
  ["Bora", "Alex", "20,000"],
  ["Chris", "Alex", "20,000"],
  ["Dana", "Bora", "8,000"]
];

export default function HomePage() {
  return (
    <main className="mx-auto grid min-h-[calc(100vh-4rem)] w-full max-w-6xl items-center gap-8 px-4 py-10 lg:grid-cols-[1fr_420px]">
      <section className="max-w-2xl">
        <p className="mb-3 text-sm font-bold uppercase tracking-[0.12em] text-mint">Meetup settlement toolkit</p>
        <h1 className="text-4xl font-black leading-tight text-ink sm:text-6xl">
          MeetSettle
        </h1>
        <p className="mt-5 max-w-xl text-lg leading-8 text-slate-600">
          Create a meetup, invite guests, record shared expenses, and turn everyone&apos;s payments into the fewest practical transfers.
        </p>
        <div className="mt-8 flex flex-wrap gap-3">
          <Link className="rounded-md bg-ink px-5 py-3 text-sm font-bold text-white shadow-soft transition hover:bg-slate-700" href="/meetups/new">
            Create meetup
          </Link>
          <a className="rounded-md border border-slate-300 bg-white px-5 py-3 text-sm font-bold text-ink transition hover:bg-slate-50" href="https://github.com/sbpark-escape/meetsettle">
            GitHub
          </a>
        </div>
      </section>

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-soft">
        <div className="mb-4 flex items-center justify-between gap-3">
          <div>
            <p className="text-sm font-bold text-slate-500">Friday dinner</p>
            <h2 className="text-2xl font-black text-ink">Settlement</h2>
          </div>
          <span className="rounded-md bg-mint/10 px-3 py-1 text-sm font-bold text-mint">KRW</span>
        </div>
        <div className="space-y-3">
          {sampleTransfers.map(([from, to, amount]) => (
            <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-3 rounded-md border border-slate-200 p-3" key={`${from}-${to}`}>
              <span className="font-bold text-slate-800">{from}</span>
              <span className="rounded-full bg-coral/10 px-3 py-1 text-sm font-bold text-coral">{amount}</span>
              <span className="text-right font-bold text-slate-800">{to}</span>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
