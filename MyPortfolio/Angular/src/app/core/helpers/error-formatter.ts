/**
 * Error Formatter Helper
 * Translates technical exceptions, database terminology, and HTTP error payloads
 * into clean, user-friendly messages for end users and clients.
 */

export function formatFriendlyError(err: any, fallbackMessage: string = 'Something went wrong. Please try again.'): string {
  if (!err) return fallbackMessage;

  // If err is already a string
  if (typeof err === 'string') {
    return sanitizeTechnicalText(err, fallbackMessage);
  }

  // Handle standard HTTP statuses
  const status = err.status;
  if (status === 0) {
    return 'Could not connect to the server. Check your internet connection and try again.';
  }
  if (status === 401) {
    return 'Your session expired. Sign in again to continue.';
  }
  if (status === 403) {
    return 'You do not have permission to perform this action.';
  }
  if (status === 404) {
    return 'The requested item was not found or has already been deleted.';
  }
  if (status === 409) {
    return 'This name or record already exists. Choose another value.';
  }

  // Handle ASP.NET Core ModelState / FluentValidation errors dictionary
  if (err.error?.errors && typeof err.error.errors === 'object') {
    const errorEntries = Object.values(err.error.errors);
    const messages: string[] = [];
    for (const val of errorEntries) {
      if (Array.isArray(val)) {
        messages.push(...val);
      } else if (typeof val === 'string') {
        messages.push(val);
      }
    }
    if (messages.length > 0) {
      return sanitizeTechnicalText(messages.join(' • '), 'Review the required fields and check your entries.');
    }
  }

  // Handle ApiResponse { message: "...", succeeded: false } or standard err.error.message
  const rawMsg = err.error?.message || err.error?.title || err.message;
  if (rawMsg && typeof rawMsg === 'string') {
    return sanitizeTechnicalText(rawMsg, fallbackMessage);
  }

  return fallbackMessage;
}

export function sanitizeTechnicalText(text: string, fallbackMessage: string): string {
  if (!text) return fallbackMessage;

  const lower = text.toLowerCase();

  // Technical database & framework jargon that must NEVER be shown to clients
  const technicalSignatures = [
    'npgsql', 'postgres', 'sql', 'database', 'inner exception', 'stack trace',
    'entity', 'typeinitializationexception', 'argumentexception', 'nullreference',
    'foreign key', 'constraint', 'column', 'relation', 'cannot insert', 'syntax error',
    'table', 'system.', 'microsoft.', 'ef core', 'dbcontext', 'duplicate key'
  ];

  const hasTechnical = technicalSignatures.some(sig => lower.includes(sig));
  if (hasTechnical) {
    if (lower.includes('duplicate') || lower.includes('already exists') || lower.includes('unique')) {
      return 'This item already exists. Choose another name or value.';
    }
    return 'There was a problem processing the data. Check your entries and try again.';
  }

  return text;
}
