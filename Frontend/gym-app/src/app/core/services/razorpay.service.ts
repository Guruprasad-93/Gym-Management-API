import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, from, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreateRazorpayOrderRequest,
  RazorpayCheckoutContext,
  RazorpayOrder,
  RefundPaymentRequest,
  RefundPaymentResult,
  VerifyRazorpayPaymentRequest,
  Payment,
} from '../../shared/models/membership-payment.models';

declare global {
  interface Window {
    Razorpay?: new (options: RazorpayCheckoutOptions) => RazorpayInstance;
  }
}

interface RazorpayCheckoutOptions {
  key: string;
  amount: number;
  currency: string;
  name: string;
  description?: string;
  order_id: string;
  prefill?: { name?: string; email?: string };
  handler: (response: RazorpaySuccessResponse) => void;
  modal?: { ondismiss?: () => void };
}

interface RazorpaySuccessResponse {
  razorpay_payment_id: string;
  razorpay_order_id: string;
  razorpay_signature: string;
}

interface RazorpayInstance {
  open(): void;
}

@Injectable({ providedIn: 'root' })
export class RazorpayService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/payments/razorpay`;
  private scriptLoaded = false;

  getCheckoutContext(): Observable<ApiResponse<RazorpayCheckoutContext>> {
    return this.http.get<ApiResponse<RazorpayCheckoutContext>>(`${this.base}/checkout-context`);
  }

  createOrder(dto: CreateRazorpayOrderRequest): Observable<ApiResponse<RazorpayOrder>> {
    return this.http.post<ApiResponse<RazorpayOrder>>(`${this.base}/order`, dto);
  }

  verifyPayment(dto: VerifyRazorpayPaymentRequest): Observable<ApiResponse<Payment>> {
    return this.http.post<ApiResponse<Payment>>(`${this.base}/verify`, dto);
  }

  refund(paymentId: number, dto: RefundPaymentRequest): Observable<ApiResponse<RefundPaymentResult>> {
    return this.http.post<ApiResponse<RefundPaymentResult>>(`${environment.apiUrl}/payments/${paymentId}/refund`, dto);
  }

  openCheckout(order: RazorpayOrder, onSuccess: (response: RazorpaySuccessResponse) => void, onDismiss?: () => void): Observable<void> {
    return this.loadScript().pipe(
      switchMap(() => {
        if (!window.Razorpay) {
          throw new Error('Razorpay checkout script failed to load.');
        }
        const instance = new window.Razorpay({
          key: order.keyId,
          amount: order.amountInPaise,
          currency: order.currency,
          name: 'Gym Management',
          description: order.planName ?? 'Membership payment',
          order_id: order.razorpayOrderId,
          prefill: {
            name: order.memberName,
            email: order.memberEmail,
          },
          handler: onSuccess,
          modal: { ondismiss: onDismiss },
        });
        instance.open();
        return from(Promise.resolve());
      })
    );
  }

  private loadScript(): Observable<void> {
    if (this.scriptLoaded && window.Razorpay) {
      return from(Promise.resolve());
    }
    return new Observable<void>((subscriber) => {
      const existing = document.querySelector('script[data-razorpay-checkout]');
      if (existing && window.Razorpay) {
        this.scriptLoaded = true;
        subscriber.next();
        subscriber.complete();
        return;
      }
      const script = document.createElement('script');
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.async = true;
      script.dataset['razorpayCheckout'] = 'true';
      script.onload = () => {
        this.scriptLoaded = true;
        subscriber.next();
        subscriber.complete();
      };
      script.onerror = () => subscriber.error(new Error('Failed to load Razorpay checkout script.'));
      document.body.appendChild(script);
    });
  }
}
