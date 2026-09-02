export function isProblemDetails(value) {
    return typeof value === 'object' && value !== null && ('title' in value || 'errorCode' in value);
}
