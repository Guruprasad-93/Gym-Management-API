import { DatePipe } from '@angular/common';

import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Chart, registerables } from 'chart.js';

import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';

import { NotificationService } from '../../../core/services/notification.service';

import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';

import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

import { DietComplianceSummary, DietTracking } from '../../../shared/models/member-self-service.models';



Chart.register(...registerables);



@Component({

  selector: 'app-member-diets',

  standalone: true,

  imports: [DatePipe, ReactiveFormsModule, SaasChartCardComponent, SaasKpiCardComponent],

  templateUrl: './member-diets.component.html',

  styleUrl: './member-diets.component.css',

})

export class MemberDietsComponent implements OnInit, AfterViewInit {

  @ViewChild('chartRef') chartRef?: ElementRef<HTMLCanvasElement>;



  private readonly service = inject(MemberSelfServiceService);

  private readonly notify = inject(NotificationService);

  private readonly fb = inject(FormBuilder);

  private chart?: Chart;



  diets = signal<DietTracking[]>([]);

  compliance = signal<DietComplianceSummary | null>(null);



  form = this.fb.group({

    dietPlanId: [1, Validators.required],

    mealsCompleted: [3, Validators.required],

    compliancePercentage: [100, Validators.required],

  });



  ngOnInit(): void {

    this.load();

    this.service.getDietCompliance().subscribe({

      next: (r) => {

        if (r.success) this.compliance.set(r.data ?? null);

      },

    });

  }



  ngAfterViewInit(): void {

    setTimeout(() => this.renderChart(), 0);

  }



  load(): void {

    this.service.getDiets().subscribe({

      next: (r) => {

        if (r.success && r.data) {

          this.diets.set(r.data);

          setTimeout(() => this.renderChart(), 0);

        }

      },

    });

  }



  save(): void {

    const v = this.form.getRawValue();

    this.service.upsertDiet({

      dietPlanId: v.dietPlanId!,

      mealsCompleted: v.mealsCompleted!,

      compliancePercentage: v.compliancePercentage!,

    }).subscribe({

      next: () => {

        this.notify.success('Diet logged');

        this.load();

      },

      error: () => this.notify.error('Failed to save diet'),

    });

  }



  private renderChart(): void {

    const el = this.chartRef?.nativeElement;

    const data = this.diets();

    if (!el || !data.length) return;

    this.chart?.destroy();

    this.chart = new Chart(el, {

      type: 'line',

      data: {

        labels: data.map((d) => d.trackingDate),

        datasets: [{

          label: 'Compliance %',

          data: data.map((d) => d.compliancePercentage),

          borderColor: '#12b76a',

          backgroundColor: 'rgba(18, 183, 106, 0.12)',

          fill: true,

          tension: 0.35,

        }],

      },

      options: {

        responsive: true,

        maintainAspectRatio: false,

        plugins: { legend: { display: false } },

        scales: { y: { max: 100 } },

      },

    });

  }

}

