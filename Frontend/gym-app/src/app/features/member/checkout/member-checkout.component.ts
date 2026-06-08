import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { RazorpayService } from '../../../core/services/razorpay.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaymentService } from '../../../core/services/payment.service';
import { RazorpayCheckoutContext } from '../../../shared/models/membership-payment.models';

@Component({
  selector: 'app-member-checkout',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './member-checkout.component.html',
  styleUrl: './member-checkout.component.css',
})
export class MemberCheckoutComponent implements OnInit {
  private readonly razorpay = inject(RazorpayService);
  private readonly paymentService = inject(PaymentService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);

  loading = signal(true);
  paying = signal(false);
  context = signal<RazorpayCheckoutContext | null>(null);

  ngOnInit(): void {
    this.razorpay.getCheckoutContext().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.context.set(res.data);
      },
      error: (e) => {
        this.loading.set(false);
        this.notify.error(e.error?.message ?? 'Unable to load checkout details');
      },
    });
  }

  pay(): void {
    const ctx = this.context();
    if (!ctx) return;

    this.paying.set(true);
    this.razorpay.createOrder({ membershipId: ctx.membershipId, renewOnSuccess: true }).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.paying.set(false);
          this.notify.error(res.message ?? 'Unable to create order');
          return;
        }
        this.razorpay.openCheckout(
          res.data,
          (response) => this.verify(response),
          () => this.paying.set(false),
        ).subscribe({
          error: () => {
            this.paying.set(false);
            this.notify.error('Unable to open Razorpay checkout');
          },
        });
      },
      error: (e) => {
        this.paying.set(false);
        this.notify.error(e.error?.message ?? 'Order creation failed');
      },
    });
  }

  private verify(response: {
    razorpay_payment_id: string;
    razorpay_order_id: string;
    razorpay_signature: string;
  }): void {
    this.razorpay.verifyPayment({
      razorpayOrderId: response.razorpay_order_id,
      razorpayPaymentId: response.razorpay_payment_id,
      razorpaySignature: response.razorpay_signature,
    }).subscribe({
      next: (res) => {
        this.paying.set(false);
        if (res.success && res.data) {
          this.notify.success('Payment successful. Membership updated.');
          this.paymentService.generateInvoice(res.data.id).subscribe({
            next: (inv) => {
              if (inv.success && inv.data) {
                this.paymentService.downloadInvoice(inv.data.id).subscribe({
                  next: (blob) => {
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `${inv.data!.invoiceNumber}.pdf`;
                    a.click();
                    URL.revokeObjectURL(url);
                  },
                });
              }
            },
          });
          void this.router.navigate(['/member']);
        }
      },
      error: (e) => {
        this.paying.set(false);
        this.notify.error(e.error?.message ?? 'Payment verification failed');
      },
    });
  }
}
