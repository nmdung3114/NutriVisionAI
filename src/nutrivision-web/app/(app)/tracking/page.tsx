import type { Metadata } from "next";
import { TrackingPage } from "@/features/tracking/tracking-page";

export const metadata: Metadata = { title: "Cân nặng & nước uống" };

export default function Page() {
  return <TrackingPage />;
}
