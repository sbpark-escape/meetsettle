// 파일 용도: 새 모임 생성 화면을 구성한다.
// 파일 목적: 모임 생성 진입점을 폼과 짧은 맥락으로 제공한다.
import { CreateMeetupForm } from "@/components/create-meetup-form";

export default function NewMeetupPage() {
  return (
    <main className="mx-auto grid w-full max-w-5xl gap-8 px-4 py-10 lg:grid-cols-[1fr_420px]">
      <section>
        <p className="mb-3 text-sm font-bold uppercase tracking-[0.12em] text-mint">New meetup</p>
        <h1 className="text-3xl font-black text-ink sm:text-5xl">Create a shared settlement room</h1>
        <p className="mt-4 max-w-xl text-lg leading-8 text-slate-600">
          Start with the meetup details, then add participants, expenses, and invite links through the API-backed flow.
        </p>
      </section>
      <CreateMeetupForm />
    </main>
  );
}
