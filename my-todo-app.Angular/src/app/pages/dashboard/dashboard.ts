import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProgressBarModule } from 'primeng/progressbar';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TaskService } from '../../core/services/task.service';
import { TodoTask } from '../../core/models/task.model';
import { statusLabel, statusSeverity, priorityLabel, prioritySeverity, formatDue, isOverdue } from '../../shared/utils/task-display.utils';

@Component({
  selector: 'app-dashboard',
  imports: [TagModule, ProgressBarModule, ProgressSpinnerModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  stats = [
    { label: 'Total Tasks',   value: 0, sub: 'Across all lists',    icon: 'pi pi-list-check',   type: 'info',    pct: null as number | null },
    { label: 'Completed',     value: 0, sub: '0% completion rate',  icon: 'pi pi-check-circle', type: 'success', pct: 0   as number | null },
    { label: 'In Progress',   value: 0, sub: 'Of 0 total tasks',    icon: 'pi pi-hourglass',    type: 'warning', pct: 0   as number | null },
    { label: 'High Priority', value: 0, sub: 'Needs attention',     icon: 'pi pi-flag',         type: 'danger',  pct: 0   as number | null },
  ];

  recentTasks: TodoTask[] = [];
  loading = false;

  constructor(private taskService: TaskService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loading = true;
    this.taskService.getAll().subscribe({
      next: tasks => { this.computeStats(tasks); this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
  }

  private computeStats(tasks: TodoTask[]) {
    const total      = tasks.length;
    const completed  = tasks.filter(t => t.isCompleted).length;
    const inProgress = tasks.filter(t => t.status === 4).length;
    const highPri    = tasks.filter(t => (t.priority ?? 0) >= 3).length;

    const completionPct  = total > 0 ? Math.round((completed  / total) * 100) : 0;
    const inProgressPct  = total > 0 ? Math.round((inProgress / total) * 100) : 0;
    const highPriPct     = total > 0 ? Math.round((highPri    / total) * 100) : 0;

    this.stats = [
      { label: 'Total Tasks',   value: total,      sub: 'Across all lists',                    icon: 'pi pi-list-check',   type: 'info',    pct: null },
      { label: 'Completed',     value: completed,  sub: `${completionPct}% completion rate`,   icon: 'pi pi-check-circle', type: 'success', pct: completionPct },
      { label: 'In Progress',   value: inProgress, sub: `Of ${total} total tasks`,             icon: 'pi pi-hourglass',    type: 'warning', pct: inProgressPct },
      { label: 'High Priority', value: highPri,    sub: 'Needs attention',                     icon: 'pi pi-flag',         type: 'danger',  pct: highPriPct },
    ];

    this.recentTasks = [...tasks]
      .sort((a, b) => new Date(b.createdDate ?? 0).getTime() - new Date(a.createdDate ?? 0).getTime())
      .slice(0, 5);
  }

  readonly statusLabel = statusLabel;
  readonly statusSeverity = statusSeverity;
  readonly priorityLabel = priorityLabel;
  readonly prioritySeverity = prioritySeverity;
  readonly formatDue = formatDue;
  readonly isOverdue = isOverdue;
}
