export function toCamelCase(
  obj?: Record<string, unknown>
): Record<string, unknown> | undefined {
  if (!obj) return obj;

  return Object.keys(obj).reduce((result, key) => {
    const camelCaseKey = key.replace(/_([a-z])/g, (g) => g[1].toUpperCase());
    result[camelCaseKey] = obj[key];
    return result;
  }, {} as Record<string, unknown>);
}
