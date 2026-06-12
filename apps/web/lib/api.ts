// 파일 용도: Next.js 프론트엔드에서 ASP.NET Core API를 호출하는 함수를 제공한다.
// 파일 목적: 화면 컴포넌트가 API 주소와 응답 타입을 직접 다루지 않도록 분리한다.
export const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5076";

export type CreateMeetupInput = {
  name: string;
  date: string;
  location?: string;
  currency?: string;
};

export type MeetupDetail = {
  id: string;
  name: string;
  date: string;
  location?: string | null;
  currency: string;
  participants: Array<{ id: string; name: string; isAttending: boolean }>;
  expenses: Array<{
    id: string;
    title: string;
    payerParticipantId: string;
    amount: number;
    shares: Array<{ participantId: string; weight: number }>;
  }>;
};

export type Settlement = {
  meetupId: string;
  currency: string;
  balances: Array<{
    participantId: string;
    participantName: string;
    paidAmount: number;
    owedAmount: number;
    netAmount: number;
  }>;
  transfers: Array<{
    id: string;
    fromParticipantId: string;
    fromParticipantName: string;
    toParticipantId: string;
    toParticipantName: string;
    amount: number;
    isCompleted: boolean;
  }>;
};

export async function createMeetup(input: CreateMeetupInput) {
  const response = await fetch(`${apiBaseUrl}/api/meetups`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input)
  });

  if (!response.ok) {
    throw new Error("Failed to create meetup");
  }

  return response.json() as Promise<{ id: string }>;
}

export async function getMeetup(id: string) {
  const response = await fetch(`${apiBaseUrl}/api/meetups/${id}`, { cache: "no-store" });

  if (!response.ok) {
    return null;
  }

  return response.json() as Promise<MeetupDetail>;
}

export async function getSettlement(id: string) {
  const response = await fetch(`${apiBaseUrl}/api/meetups/${id}/settlement`, { cache: "no-store" });

  if (!response.ok) {
    return null;
  }

  return response.json() as Promise<Settlement>;
}
