export function statusLabel(s: number | null): string {
  if (s == null) return '';
  const map: Record<number, string> = {
    1: 'Created',
    2: 'Started',
    3: 'Pending',
    4: 'In Progress',
    5: 'Completed',
    6: 'Cancelled',
    7: 'On Hold',
  };
  return map[s] ?? '';
}

export function statusSeverity(s: number | null): 'secondary' | 'info' | 'warn' | 'success' | 'danger' {
  if (s === 5) return 'success';
  if (s === 4) return 'info';
  if (s === 3) return 'warn';
  if (s === 6) return 'danger';
  return 'secondary';
}

export function priorityLabel(p: number | null): string {
  if (p == null) return '';
  return (['', 'Low', 'Medium', 'High', 'Critical'] as const)[p] ?? '';
}

export function prioritySeverity(p: number | null): 'secondary' | 'info' | 'warn' | 'danger' {
  if (p === 4) return 'danger';
  if (p === 3) return 'warn';
  if (p === 2) return 'info';
  return 'secondary';
}

export function formatDue(iso: string | null | undefined): string {
  if (!iso) return '';
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

export function isOverdue(iso: string | null | undefined, isCompleted: boolean): boolean {
  if (!iso || isCompleted) return false;
  return new Date(iso) < new Date();
}
