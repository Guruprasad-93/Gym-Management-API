import { Component, OnInit, inject } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PrivilegeService } from '../../../core/services/privilege.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { RolePermissionMatrix } from '../../../shared/models/role.models';

@Component({
  selector: 'app-role-matrix',
  standalone: true,
  imports: [MatTableModule, MatCheckboxModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header
      title="Role–Privilege Matrix"
      subtitle="Assign or remove privileges per role" />

    @if (loading) {
      <mat-spinner />
    } @else if (matrix) {
      <div class="matrix-wrap">
        <table class="matrix-table">
          <thead>
            <tr>
              <th class="sticky-col">Privilege</th>
              @for (role of matrix.roles; track role.roleId) {
                <th>{{ role.roleName }}</th>
              }
            </tr>
          </thead>
          <tbody>
            @for (priv of matrix.privileges; track priv.privilegeId) {
              <tr>
                <td class="sticky-col">
                  <strong>{{ priv.privilegeName }}</strong>
                  <small>{{ priv.category }}</small>
                </td>
                @for (role of matrix.roles; track role.roleId) {
                  <td class="cell-center">
                    <mat-checkbox
                      [checked]="isAssigned(role.roleId, priv.privilegeId)"
                      [disabled]="!canAssign()"
                      (change)="toggle(role.roleId, priv.privilegeId, $event.checked)" />
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
  styles: [
    `
      .matrix-wrap {
        overflow: auto;
        background: #fff;
        border-radius: 8px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      }
      .matrix-table {
        border-collapse: collapse;
        min-width: 100%;
      }
      th,
      td {
        border: 1px solid #eee;
        padding: 0.5rem;
        font-size: 0.85rem;
      }
      th {
        background: #f5f5f5;
        white-space: nowrap;
      }
      .sticky-col {
        position: sticky;
        left: 0;
        background: #fff;
        z-index: 1;
        min-width: 200px;
      }
      .sticky-col small {
        display: block;
        color: #888;
      }
      .cell-center {
        text-align: center;
      }
    `,
  ],
})
export class RoleMatrixComponent implements OnInit {
  private readonly privilegeService = inject(PrivilegeService);
  private readonly notify = inject(NotificationService);
  private readonly auth = inject(AuthService);

  matrix: RolePermissionMatrix | null = null;
  loading = true;
  private assignmentMap = new Map<string, boolean>();

  ngOnInit(): void {
    this.load();
  }

  canAssign(): boolean {
    return (
      this.auth.hasPermission(Permissions.AssignRolePrivilege) ||
      this.auth.hasPermission(Permissions.RemoveRolePrivilege)
    );
  }

  load(): void {
    this.loading = true;
    this.privilegeService.getMatrix().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.matrix = res.data;
          this.assignmentMap.clear();
          for (const a of res.data.assignments) {
            this.assignmentMap.set(`${a.roleId}-${a.privilegeId}`, a.assigned);
          }
        }
      },
      error: () => {
        this.loading = false;
        this.notify.error('Failed to load matrix');
      },
    });
  }

  isAssigned(roleId: number, privilegeId: number): boolean {
    return this.assignmentMap.get(`${roleId}-${privilegeId}`) ?? false;
  }

  toggle(roleId: number, privilegeId: number, checked: boolean): void {
    const req = checked
      ? this.privilegeService.assign(roleId, privilegeId)
      : this.privilegeService.remove(roleId, privilegeId);
    req.subscribe({
      next: (res) => {
        if (res.success) {
          this.assignmentMap.set(`${roleId}-${privilegeId}`, checked);
          this.notify.success(checked ? 'Privilege assigned' : 'Privilege removed');
        }
      },
      error: () => {
        this.notify.error('Update failed');
        this.load();
      },
    });
  }
}
