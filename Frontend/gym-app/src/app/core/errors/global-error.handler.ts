import { ErrorHandler, Injectable, Injector, inject } from '@angular/core';
import { NotificationService } from '../services/notification.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly injector = inject(Injector);

  handleError(error: unknown): void {
    console.error('Unhandled application error:', error);

    const message =
      error instanceof Error
        ? error.message
        : 'An unexpected application error occurred.';

    if (message.includes('ExpressionChangedAfterItHasBeenChecked')) {
      return;
    }

    // Resolve lazily to avoid NG0200 (ErrorHandler ↔ Toastr ↔ ApplicationRef cycle at bootstrap).
    try {
      this.injector.get(NotificationService).error(message);
    } catch {
      // Notification layer not ready during early bootstrap failures.
    }
  }
}
