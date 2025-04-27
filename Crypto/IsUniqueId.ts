const uniqueIdRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function isUniqueId(maybeId: any): maybeId is string {
    return maybeId !== null && maybeId !== undefined && typeof maybeId === "string" && uniqueIdRegex.test(maybeId);
}