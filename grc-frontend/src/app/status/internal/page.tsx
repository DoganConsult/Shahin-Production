"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

type Badge = "green" | "yellow" | "red";

type Result = {
  key: string;
  name: string;
  group: string;
  checkType: "http" | "tcp";
  target: string;
  ok: boolean;
  ms: number;
  badge: Badge;
  httpStatus?: number;
  error?: string;
};

type StatusData = {
  mode: string;
  checkedAt: string;
  refreshSeconds: number;
  results: Result[];
};

function groupBy<T>(arr: T[], keyFn: (x: T) => string): Record<string, T[]> {
  return arr.reduce<Record<string, T[]>>((acc, x) => {
    const k = keyFn(x);
    acc[k] = acc[k] || [];
    acc[k].push(x);
    return acc;
  }, {});
}

function BadgePill({ badge }: { badge: Badge }) {
  const label =
    badge === "green" ? "Healthy" : badge === "yellow" ? "Degraded" : "Down";
  const cls =
    badge === "green"
      ? "bg-green-600/15 text-green-700 dark:bg-green-500/20 dark:text-green-400"
      : badge === "yellow"
      ? "bg-yellow-600/15 text-yellow-700 dark:bg-yellow-500/20 dark:text-yellow-400"
      : "bg-red-600/15 text-red-700 dark:bg-red-500/20 dark:text-red-400";

  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
}

function StatusGrid({ data }: { data: StatusData | null }) {
  if (!data) {
    return (
      <div className="flex items-center justify-center h-32">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        <span className="ml-3 text-muted-foreground">Loading status...</span>
      </div>
    );
  }

  const grouped = groupBy<Result>(data.results, (r) => r.group);

  // Calculate overall status
  const allHealthy = data.results.every((r) => r.badge === "green");
  const anyDown = data.results.some((r) => r.badge === "red");
  const overallStatus = anyDown ? "red" : allHealthy ? "green" : "yellow";

  return (
    <div className="space-y-6">
      {/* Overall Status Banner */}
      <div
        className={`rounded-xl p-4 ${
          overallStatus === "green"
            ? "bg-green-50 border border-green-200 dark:bg-green-950/30 dark:border-green-800"
            : overallStatus === "yellow"
            ? "bg-yellow-50 border border-yellow-200 dark:bg-yellow-950/30 dark:border-yellow-800"
            : "bg-red-50 border border-red-200 dark:bg-red-950/30 dark:border-red-800"
        }`}
      >
        <div className="flex items-center gap-3">
          <div
            className={`w-3 h-3 rounded-full ${
              overallStatus === "green"
                ? "bg-green-500"
                : overallStatus === "yellow"
                ? "bg-yellow-500"
                : "bg-red-500"
            }`}
          ></div>
          <span className="font-medium">
            {overallStatus === "green"
              ? "All systems operational"
              : overallStatus === "yellow"
              ? "Some systems degraded"
              : "System outage detected"}
          </span>
        </div>
      </div>

      {/* Mode Indicator */}
      <div className="rounded-lg bg-blue-50 border border-blue-200 dark:bg-blue-950/30 dark:border-blue-800 p-3">
        <p className="text-sm text-blue-700 dark:text-blue-400">
          <strong>Internal Mode:</strong> Checking Docker service DNS
          (container-to-container). This works when running inside Docker
          network.
        </p>
      </div>

      {/* Last Checked */}
      <div className="text-sm text-muted-foreground">
        Last checked: {new Date(data.checkedAt).toLocaleString()} (refreshes
        every {data.refreshSeconds}s)
      </div>

      {/* Service Groups */}
      {Object.entries(grouped).map(([group, items]) => (
        <section key={group} className="space-y-3">
          <h2 className="text-lg font-semibold">{group}</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {items.map((r) => (
              <div
                key={r.key}
                className="rounded-xl border bg-card p-4 shadow-sm hover:shadow-md transition-shadow"
              >
                <div className="flex items-center justify-between">
                  <div className="font-medium">{r.name}</div>
                  <BadgePill badge={r.badge} />
                </div>
                <div className="mt-2 text-sm text-muted-foreground">
                  {r.checkType.toUpperCase()} &bull; {r.target}
                </div>
                <div className="mt-2 text-sm flex items-center gap-2">
                  <span
                    className={`font-medium ${
                      r.ok ? "text-green-600" : "text-red-600"
                    }`}
                  >
                    {r.ok ? "OK" : "FAIL"}
                  </span>
                  <span className="text-muted-foreground">{r.ms} ms</span>
                  {r.httpStatus ? (
                    <span className="text-muted-foreground">
                      HTTP {r.httpStatus}
                    </span>
                  ) : null}
                </div>
                {r.error ? (
                  <div className="mt-2 text-xs text-red-600 dark:text-red-400">
                    Error: {r.error}
                  </div>
                ) : null}
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

export default function StatusInternalPage() {
  const [data, setData] = useState<StatusData | null>(null);

  async function loadStatus() {
    try {
      const res = await fetch("/api/status?mode=internal", {
        cache: "no-store",
      });
      const json = await res.json();
      setData(json);
    } catch (err) {
      console.error("Failed to fetch status:", err);
    }
  }

  useEffect(() => {
    loadStatus();
    const interval = setInterval(loadStatus, 15000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold">Service Status (Internal)</h1>
            <p className="text-sm text-muted-foreground">
              Checks Docker service DNS (container mode)
            </p>
          </div>
          <div className="flex gap-4">
            <Link
              href="/status"
              className="text-sm text-primary underline underline-offset-4 hover:text-primary/80"
            >
              Switch to External
            </Link>
            <Link
              href="/"
              className="text-sm text-muted-foreground underline underline-offset-4 hover:text-foreground"
            >
              Back to Home
            </Link>
          </div>
        </div>

        {/* Status Grid */}
        <StatusGrid data={data} />
      </div>
    </div>
  );
}
