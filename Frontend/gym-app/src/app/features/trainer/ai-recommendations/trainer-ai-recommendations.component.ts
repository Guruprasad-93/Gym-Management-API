import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AiService } from '../../../core/services/ai.service';
import { AiRecommendation } from '../../../shared/models/ai.models';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

@Component({
  selector: 'app-trainer-ai-recommendations',
  standalone: true,
  imports: [DatePipe, FormsModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule, SaasKpiCardComponent],
  templateUrl: './trainer-ai-recommendations.component.html',
  styleUrl: './trainer-ai-recommendations.component.css',
})
export class TrainerAiRecommendationsComponent implements OnInit {
  private readonly svc = inject(AiService);
  private readonly snack = inject(MatSnackBar);

  recommendations: AiRecommendation[] = [];
  loading = true;
  search = '';
  typeFilter = 'all';
  statusFilter = 'all';

  get types(): string[] {
    return [...new Set(this.recommendations.map((r) => r.recommendationType))].sort();
  }

  get filteredRecommendations(): AiRecommendation[] {
    const q = this.search.trim().toLowerCase();
    return this.recommendations.filter((r) => {
      const matchesType = this.typeFilter === 'all' || r.recommendationType === this.typeFilter;
      const matchesStatus =
        this.statusFilter === 'all' ||
        (this.statusFilter === 'accepted' && r.isAccepted) ||
        (this.statusFilter === 'pending' && !r.isAccepted);
      const matchesSearch =
        !q ||
        r.memberName.toLowerCase().includes(q) ||
        r.recommendationText.toLowerCase().includes(q) ||
        r.recommendationType.toLowerCase().includes(q);
      return matchesType && matchesStatus && matchesSearch;
    });
  }

  get pendingCount(): number {
    return this.recommendations.filter((r) => !r.isAccepted).length;
  }

  get acceptedCount(): number {
    return this.recommendations.filter((r) => r.isAccepted).length;
  }

  ngOnInit(): void {
    this.load();
  }

  accept(id: number): void {
    this.svc.acceptRecommendation(id).subscribe({
      next: () => {
        this.snack.open('Recommendation marked as accepted.', 'OK', { duration: 2500 });
        this.load();
      },
      error: () => this.snack.open('Failed to update recommendation.', 'Dismiss', { duration: 3000 }),
    });
  }

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  private load(): void {
    this.loading = true;
    this.svc.getRecommendations(1, 30).subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success && r.data) this.recommendations = r.data.items;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
