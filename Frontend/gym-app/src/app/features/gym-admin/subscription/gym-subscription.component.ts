import { CurrencyPipe } from '@angular/common';

import { Component, OnInit, inject, signal } from '@angular/core';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

import { SaasSubscriptionService } from '../../../core/services/saas-subscription.service';

import { NotificationService } from '../../../core/services/notification.service';

import { RazorpayService } from '../../../core/services/razorpay.service';

import { GymSubscription, GymUsage, SaasPlan } from '../../../shared/models/saas.models';



@Component({

  selector: 'app-gym-subscription',

  standalone: true,

  imports: [CurrencyPipe, MatProgressSpinnerModule, SaasKpiCardComponent],

  templateUrl: './gym-subscription.component.html',

  styleUrl: './gym-subscription.component.css',

})

export class GymSubscriptionComponent implements OnInit {

  private readonly saas = inject(SaasSubscriptionService);

  private readonly razorpay = inject(RazorpayService);

  private readonly notify = inject(NotificationService);



  loading = signal(true);

  paying = signal(false);

  subscription = signal<GymSubscription | null>(null);

  usage = signal<GymUsage | null>(null);

  plans = signal<SaasPlan[]>([]);



  ngOnInit(): void {

    this.saas.getPlans().subscribe({

      next: (res) => {

        if (res.success && res.data) this.plans.set(res.data);

      },

    });

    this.saas.getSubscription().subscribe({

      next: (res) => {

        this.loading.set(false);

        if (res.success && res.data) this.subscription.set(res.data);

      },

      error: () => {

        this.loading.set(false);

        this.notify.error('Failed to load subscription');

      },

    });

    this.saas.getUsage().subscribe({

      next: (res) => {

        if (res.success && res.data) this.usage.set(res.data);

      },

    });

  }



  upgrade(planId: number, billingCycle: string): void {

    this.paying.set(true);

    this.saas.createPaymentOrder(planId, billingCycle).subscribe({

      next: async (res) => {

        if (!res.success || !res.data) {

          this.paying.set(false);

          return;

        }

        try {

          const order = {

            paymentId: res.data.saasPaymentId,

            razorpayOrderId: res.data.razorpayOrderId,

            amount: res.data.amount,

            amountInPaise: Math.round(res.data.amount * 100),

            currency: res.data.currency,

            keyId: res.data.keyId,

            planName: res.data.planName,

            memberName: '',

            memberEmail: '',

          };

          this.razorpay

            .openCheckout(

              order,

              (payment) => {

                this.saas

                  .verifyPayment({

                    saasPaymentId: res.data!.saasPaymentId,

                    razorpayOrderId: payment.razorpay_order_id,

                    razorpayPaymentId: payment.razorpay_payment_id,

                    razorpaySignature: payment.razorpay_signature,

                  })

                  .subscribe({

                    next: (verify) => {

                      this.paying.set(false);

                      if (verify.success && verify.data) {

                        this.subscription.set(verify.data);

                        this.notify.success('Subscription upgraded successfully');

                      }

                    },

                    error: () => {

                      this.paying.set(false);

                      this.notify.error('Payment verification failed');

                    },

                  });

              },

              () => this.paying.set(false),

            )

            .subscribe();

        } catch {

          this.paying.set(false);

        }

      },

      error: () => {

        this.paying.set(false);

        this.notify.error('Unable to start payment');

      },

    });

  }



  cancel(): void {

    this.saas.cancel(true).subscribe({

      next: (res) => {

        if (res.success && res.data) {

          this.subscription.set(res.data);

          this.notify.success('Subscription will cancel at period end');

        }

      },

      error: () => this.notify.error('Cancel failed'),

    });

  }

}

