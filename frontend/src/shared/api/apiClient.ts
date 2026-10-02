import { ApiError, type ProblemDetails } from "./apiError";
export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body) headers.set("Content-Type", "application/json");
  let response: Response;
  try {
    response = await fetch(
      `${(import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/$/, "")}${path}`,
      { ...options, headers },
    );
  } catch {
    throw new ApiError(
      "Cannot reach the server. Check your connection and try again.",
      0,
    );
  }
  if (!response.ok) {
    const body: unknown = await response.json().catch(() => null);
    const problem =
      body && typeof body === "object" ? (body as ProblemDetails) : null;
    const fields = problem?.errors
      ? Object.values(problem.errors).flat().join(" ")
      : "";
    const fallback =
      response.status === 404
        ? "The requested record was not found."
        : response.status >= 500
          ? "The server could not complete the request. Please try again."
          : `The request failed (${response.status}).`;
    throw new ApiError(
      [problem?.detail || problem?.title || fallback, fields]
        .filter(Boolean)
        .join(" "),
      response.status,
      body,
    );
  }
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}
