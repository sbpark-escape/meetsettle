// 파일 용도: 새 모임 생성 폼의 클라이언트 상호작용을 담당한다.
// 파일 목적: 사용자가 입력한 모임 정보를 API에 전달하고 생성 후 상세 화면으로 이동시킨다.
"use client";

import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { createMeetup } from "@/lib/api";

export function CreateMeetupForm() {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const formData = new FormData(event.currentTarget);

    try {
      const meetup = await createMeetup({
        name: String(formData.get("name") ?? ""),
        date: String(formData.get("date") ?? ""),
        location: String(formData.get("location") ?? ""),
        currency: String(formData.get("currency") ?? "KRW")
      });

      router.push(`/meetups/${meetup.id}`);
    } catch {
      setError("API is not reachable. Check NEXT_PUBLIC_API_BASE_URL and run the backend.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="space-y-4 rounded-lg border border-slate-200 bg-white p-4 shadow-soft" onSubmit={handleSubmit}>
      <label className="block">
        <span className="mb-2 block text-sm font-bold text-slate-700">Meetup name</span>
        <input className="w-full rounded-md border border-slate-300 px-3 py-3 outline-none focus:border-mint" name="name" placeholder="Friday dinner" required />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-bold text-slate-700">Date</span>
        <input className="w-full rounded-md border border-slate-300 px-3 py-3 outline-none focus:border-mint" name="date" required type="date" />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-bold text-slate-700">Location</span>
        <input className="w-full rounded-md border border-slate-300 px-3 py-3 outline-none focus:border-mint" name="location" placeholder="Seoul" />
      </label>
      <label className="block">
        <span className="mb-2 block text-sm font-bold text-slate-700">Currency</span>
        <select className="w-full rounded-md border border-slate-300 px-3 py-3 outline-none focus:border-mint" defaultValue="KRW" name="currency">
          <option value="KRW">KRW</option>
          <option value="USD">USD</option>
        </select>
      </label>
      {error ? <p className="rounded-md bg-coral/10 px-3 py-2 text-sm font-semibold text-coral">{error}</p> : null}
      <button className="w-full rounded-md bg-ink px-4 py-3 font-bold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-400" disabled={isSubmitting} type="submit">
        {isSubmitting ? "Creating..." : "Create meetup"}
      </button>
    </form>
  );
}
