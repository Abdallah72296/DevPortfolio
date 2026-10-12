/**
 * Error Formatter Helper
 * Translates technical exceptions, database terminology, and HTTP error payloads
 * into clean, user-friendly messages for end users and clients.
 */

export function formatFriendlyError(err: any, fallbackMessage: string = 'حدث خطأ أثناء معالجة الطلب. يرجى المحاولة مرة أخرى.'): string {
  if (!err) return fallbackMessage;

  // If err is already a string
  if (typeof err === 'string') {
    return sanitizeTechnicalText(err, fallbackMessage);
  }

  // Handle standard HTTP statuses
  const status = err.status;
  if (status === 0) {
    return 'تعذر الاتصال بالخادم. يرجى التحقق من اتصالك بالإنترنت أو إعادة المحاولة لاحقاً.';
  }
  if (status === 401) {
    return 'انتهت صلاحية الجلسة أو يجب تسجيل الدخول مجدداً لإتمام هذا الإجراء.';
  }
  if (status === 403) {
    return 'عذراً، ليس لديك الصلاحيات الكافية لتنفيذ هذا الإجراء.';
  }
  if (status === 404) {
    return 'العنصر المطلوب غير موجود أو تم حذفه مسبقاً.';
  }
  if (status === 409) {
    return 'هذا الاسم أو السجل مضاف بالفعل، يرجى اختيار اسم آخر لتفادي التكرار.';
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
      return sanitizeTechnicalText(messages.join(' • '), 'يرجى مراجعة الحقول المطلوبة والتأكد من صحة المدخلات.');
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
      return 'هذا العنصر موجود مسبقاً، يرجى اختيار اسم أو قيمة أخرى.';
    }
    return 'حدث خطأ في معالجة البيانات، يرجى التحقق من المدخلات والمحاولة لاحقاً.';
  }

  return text;
}
