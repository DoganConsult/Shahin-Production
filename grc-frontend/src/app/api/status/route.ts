import { NextResponse } from "next/server";
import fs from "fs";
import path from "path";
import net from "net";

type CheckMode = "external" | "internal";

type ServiceConfig = {
  key: string;
  name: string;
  group: string;
  checkType: "http" | "tcp";
  // HTTP checks
  internalUrl?: string;
  externalUrl?: string;
  healthPath?: string;
  // TCP checks
  internalHost?: string;
  internalPort?: number;
  externalHost?: string;
  externalPort?: number;
  // Thresholds
  warnAfterMs?: number;
  failAfterMs?: number;
};

type EndpointConfig = {
  refreshSeconds?: number;
  services: ServiceConfig[];
};

function loadConfig(): EndpointConfig {
  const filePath = path.join(process.cwd(), "infra", "endpoints.json");
  const raw = fs.readFileSync(filePath, "utf-8");
  return JSON.parse(raw);
}

async function checkHttp(
  url: string,
  timeoutMs: number
): Promise<{ ok: boolean; status: number; ms: number; error?: string }> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  const start = Date.now();
  try {
    const res = await fetch(url, {
      signal: controller.signal,
      cache: "no-store",
    });
    const ms = Date.now() - start;
    return { ok: res.ok, status: res.status, ms };
  } catch (e: unknown) {
    const ms = Date.now() - start;
    const error = e instanceof Error ? e.name : "FetchError";
    return { ok: false, status: 0, ms, error };
  } finally {
    clearTimeout(timer);
  }
}

function checkTcp(
  host: string,
  port: number,
  timeoutMs: number
): Promise<{ ok: boolean; ms: number; error?: string }> {
  return new Promise((resolve) => {
    const start = Date.now();
    const socket = new net.Socket();

    const done = (ok: boolean, error?: string) => {
      const ms = Date.now() - start;
      try {
        socket.destroy();
      } catch {
        // Ignore errors during cleanup
      }
      resolve({ ok, ms, error });
    };

    socket.setTimeout(timeoutMs);
    socket.once("connect", () => done(true));
    socket.once("timeout", () => done(false, "Timeout"));
    socket.once("error", (err) => done(false, err.message));

    socket.connect(port, host);
  });
}

function toBadge(
  ok: boolean,
  ms: number,
  warnAfterMs = 800,
  failAfterMs = 3000
): "green" | "yellow" | "red" {
  if (!ok) return "red";
  if (ms >= failAfterMs) return "red";
  if (ms >= warnAfterMs) return "yellow";
  return "green";
}

export async function GET(req: Request) {
  const { searchParams } = new URL(req.url);
  const mode = (searchParams.get("mode") as CheckMode) || "external";

  const config = loadConfig();
  const timeoutMs = 4000;

  const results = await Promise.all(
    config.services.map(async (svc) => {
      const warnAfterMs = svc.warnAfterMs ?? 800;
      const failAfterMs = svc.failAfterMs ?? 3000;

      if (svc.checkType === "http") {
        const base = mode === "internal" ? svc.internalUrl : svc.externalUrl;
        const healthPath = svc.healthPath || "/";
        const target = `${(base || "").replace(/\/$/, "")}${healthPath}`;

        if (!base) {
          return {
            ...svc,
            mode,
            target,
            ok: false,
            ms: 0,
            badge: "red" as const,
            error: "Missing URL for mode",
          };
        }

        const r = await checkHttp(target, timeoutMs);
        const badge = toBadge(r.ok, r.ms, warnAfterMs, failAfterMs);

        return {
          ...svc,
          mode,
          target,
          ok: r.ok,
          ms: r.ms,
          httpStatus: r.status,
          badge,
          error: r.error,
        };
      }

      // TCP check
      const host = mode === "internal" ? svc.internalHost : svc.externalHost;
      const port = mode === "internal" ? svc.internalPort : svc.externalPort;

      if (!host || !port) {
        return {
          ...svc,
          mode,
          target: `${host ?? "?"}:${port ?? "?"}`,
          ok: false,
          ms: 0,
          badge: "red" as const,
          error: "Missing host/port for mode",
        };
      }

      const r = await checkTcp(host, port, timeoutMs);
      const badge = toBadge(r.ok, r.ms, warnAfterMs, failAfterMs);

      return {
        ...svc,
        mode,
        target: `${host}:${port}`,
        ok: r.ok,
        ms: r.ms,
        badge,
        error: r.error,
      };
    })
  );

  return NextResponse.json({
    mode,
    checkedAt: new Date().toISOString(),
    refreshSeconds: config.refreshSeconds ?? 15,
    results,
  });
}
