import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TaskService } from '../../../core/services/task.service';
import { ListService } from '../../../core/services/list.service';
import { TodoTask, TaskDto, TaskStatus, TaskPriority } from '../../../core/models/task.model';
import { TodoList } from '../../../core/models/list.model';
import { SubTask } from '../../../core/models/subtask.model';
import { statusLabel, statusSeverity, priorityLabel, prioritySeverity, formatDue, isOverdue } from '../../../shared/utils/task-display.utils';

interface TaskForm {
  id: string | null;
  title: string;
  description: string;
  status: number;
  priority: number;
  dueDate: Date | null;
  listId: string | null;
  newSubTask: string;
  subTasks: SubTask[];
}

@Component({
  selector: 'app-tasks',
  imports: [
    FormsModule,
    CheckboxModule, ButtonModule, DialogModule,
    InputTextModule, TextareaModule, SelectModule,
    DatePickerModule, TagModule, TooltipModule,
    ConfirmDialogModule, ProgressSpinnerModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './tasks.html',
  styleUrl: './tasks.scss',
})
export class TasksComponent implements OnInit {
  selectedListId: string | null = null;
  taskVisible = false;
  isEditing = false;
  searchQuery = '';
  taskForm: TaskForm = this.emptyForm();

  loading = false;
  savingTask = false;
  savingList = false;
  private listFromTask = false;

  listDialogVisible = false;
  listForm = { name: '', color: '#3b82f6' };

  listColors = [
    '#3b82f6', '#10b981', '#f59e0b', '#ef4444',
    '#8b5cf6', '#ec4899', '#06b6d4', '#64748b',
  ];

  statusOptions = [
    { label: 'Created',     value: 1 },
    { label: 'Started',     value: 2 },
    { label: 'Pending',     value: 3 },
    { label: 'In Progress', value: 4 },
    { label: 'Completed',   value: 5 },
    { label: 'Cancelled',   value: 6 },
    { label: 'On Hold',     value: 7 },
  ];

  priorityOptions = [
    { label: 'Low',      value: 1 },
    { label: 'Medium',   value: 2 },
    { label: 'High',     value: 3 },
    { label: 'Critical', value: 4 },
  ];

  lists: TodoList[] = [];
  allTasks: TodoTask[] = [];

  activeTab: 'all' | 'status' | 'priority' | 'completed' = 'all';
  completingTasks = new Set<string>();

  readonly statusGroups = [
    { label: 'Created',     value: 1, color: '#64748b' },
    { label: 'Started',     value: 2, color: '#3b82f6' },
    { label: 'Pending',     value: 3, color: '#f59e0b' },
    { label: 'In Progress', value: 4, color: '#8b5cf6' },
    { label: 'Cancelled',   value: 6, color: '#ef4444' },
    { label: 'On Hold',     value: 7, color: '#94a3b8' },
  ];

  readonly priorityGroups = [
    { label: 'Critical', value: 4, color: '#ef4444' },
    { label: 'High',     value: 3, color: '#f97316' },
    { label: 'Medium',   value: 2, color: '#3b82f6' },
    { label: 'Low',      value: 1, color: '#94a3b8' },
  ];

  constructor(
    private taskService: TaskService,
    private listService: ListService,
    private confirm: ConfirmationService,
    private messages: MessageService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loading = true;
    forkJoin({
      tasks: this.taskService.getAll(),
      lists: this.listService.getAll(),
    }).subscribe({
      next: ({ tasks, lists }) => {
        this.lists = lists;
        this.allTasks = tasks;
        this.syncTaskCounts();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
  }

  private syncTaskCounts() {
    this.lists = this.lists.map(l => ({
      ...l,
      taskCount: this.allTasks.filter(t => t.listId === l.id).length,
    }));
  }

  get listOptions() {
    return this.lists.map(l => ({ label: l.name, value: l.id }));
  }

  private applyListAndSearch(tasks: TodoTask[]): TodoTask[] {
    if (this.selectedListId) tasks = tasks.filter(t => t.listId === this.selectedListId);
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      tasks = tasks.filter(t =>
        t.title.toLowerCase().includes(q) ||
        t.description.toLowerCase().includes(q)
      );
    }
    return tasks;
  }

  get baseActiveTasks(): TodoTask[] {
    return this.applyListAndSearch(this.allTasks.filter(t => !t.isCompleted));
  }

  get completedTasks(): TodoTask[] {
    return this.applyListAndSearch(this.allTasks.filter(t => t.isCompleted));
  }

  get filteredTasks(): TodoTask[] {
    return this.activeTab === 'completed' ? this.completedTasks : this.baseActiveTasks;
  }

  get tasksByStatus() {
    const tasks = this.baseActiveTasks;
    return this.statusGroups
      .map(g => ({ ...g, tasks: tasks.filter(t => t.status === g.value) }))
      .filter(g => g.tasks.length > 0);
  }

  get tasksByPriority() {
    const tasks = this.baseActiveTasks;
    return this.priorityGroups
      .map(g => ({ ...g, tasks: tasks.filter(t => t.priority === g.value) }))
      .filter(g => g.tasks.length > 0);
  }

  get displayCount(): number {
    if (this.activeTab === 'completed') return this.completedTasks.length;
    return this.baseActiveTasks.length;
  }

  listName(id: string | null): string {
    return this.lists.find(l => l.id === id)?.name ?? '';
  }

  listColor(id: string | null): string {
    return this.lists.find(l => l.id === id)?.color ?? 'var(--p-primary-color)';
  }

  completedSubtasks(task: TodoTask): number {
    return task.subTasks.filter(s => s.isCompleted).length;
  }

  readonly statusLabel = statusLabel;
  readonly statusSeverity = statusSeverity;
  readonly priorityLabel = priorityLabel;
  readonly prioritySeverity = prioritySeverity;
  readonly formatDue = formatDue;
  readonly isOverdue = isOverdue;

  get statusTimeline() {
    const cur = this.taskForm.status;
    return [
      { label: 'Created',     icon: 'pi pi-circle',    value: 1 },
      { label: 'Started',     icon: 'pi pi-hourglass', value: 2 },
      { label: 'In Progress', icon: 'pi pi-sync',      value: 4 },
      { label: 'Completed',   icon: 'pi pi-check',     value: 5 },
    ].map(s => ({
      ...s,
      severity: s.value === 5 ? 'success' : s.value === cur ? 'info' : 'secondary',
    }));
  }

  emptyForm(): TaskForm {
    return {
      id: null, title: '', description: '',
      status: 1, priority: 2, dueDate: null,
      listId: this.selectedListId,
      newSubTask: '', subTasks: [],
    };
  }

  taskToForm(task: TodoTask): TaskForm {
    return {
      id: task.id,
      title: task.title,
      description: task.description,
      status: task.status ?? 1,
      priority: task.priority ?? 2,
      dueDate: task.dueDate ? new Date(task.dueDate) : null,
      listId: task.listId,
      newSubTask: '',
      subTasks: task.subTasks.map(s => ({ ...s })),
    };
  }

  openAdd() {
    this.taskForm = this.emptyForm();
    this.isEditing = false;
    this.taskVisible = true;
  }

  openEdit(task: TodoTask) {
    this.taskForm = this.taskToForm(task);
    this.isEditing = true;
    this.taskVisible = true;
  }

  toggleComplete(task: TodoTask) {
    const newCompleted = task.isCompleted;
    const newStatus: TaskStatus = newCompleted ? 5 : (task.status === 5 ? 4 : (task.status ?? 4));

    if (newCompleted) {
      this.completingTasks.add(task.id);
      this.messages.add({
        severity: 'success',
        summary: 'Task completed!',
        detail: `"${task.title}" moved to Completed`,
        life: 2500,
      });
      setTimeout(() => { this.completingTasks.delete(task.id); this.cdr.detectChanges(); }, 420);
    }

    const dto: TaskDto = {
      title: task.title,
      description: task.description,
      status: newStatus,
      priority: task.priority,
      dueDate: task.dueDate,
      isCompleted: newCompleted,
      listId: task.listId,
      subTasks: task.subTasks,
    };

    this.taskService.update(task.id, dto).subscribe({
      next: updated => {
        this.allTasks = this.allTasks.map(t => t.id === updated.id ? updated : t);
        this.cdr.detectChanges();
      },
      error: () => {
        task.isCompleted = !newCompleted;
        this.completingTasks.delete(task.id);
        this.allTasks = [...this.allTasks];
        this.cdr.detectChanges();
      },
    });
  }

  addSubTask() {
    if (!this.taskForm.newSubTask.trim()) return;
    this.taskForm.subTasks = [
      ...this.taskForm.subTasks,
      { id: null, title: this.taskForm.newSubTask.trim(), isCompleted: false, parentTaskId: this.taskForm.id ?? '' },
    ];
    this.taskForm.newSubTask = '';
  }

  removeSubTask(sub: SubTask) {
    this.taskForm.subTasks = this.taskForm.subTasks.filter(s => s !== sub);
  }

  saveTask() {
    if (!this.taskForm.title.trim()) {
      this.messages.add({ severity: 'warn', summary: 'Title required', detail: 'Please enter a task title.', life: 3000 });
      return;
    }
    if (!this.taskForm.description.trim()) {
      this.messages.add({ severity: 'warn', summary: 'Description required', detail: 'Please add a short description.', life: 3000 });
      return;
    }
    if (!this.taskForm.listId) {
      this.messages.add({ severity: 'warn', summary: 'List required', detail: 'Please select a list for this task.', life: 3000 });
      return;
    }
    this.savingTask = true;

    const dto: TaskDto = {
      title: this.taskForm.title,
      description: this.taskForm.description,
      status: this.taskForm.status as TaskStatus,
      priority: this.taskForm.priority as TaskPriority,
      dueDate: this.taskForm.dueDate ? this.taskForm.dueDate.toISOString() : null,
      isCompleted: this.taskForm.status === 5,
      listId: this.taskForm.listId,
      subTasks: this.taskForm.subTasks,
    };

    if (this.isEditing && this.taskForm.id) {
      this.taskService.update(this.taskForm.id, dto).subscribe({
        next: updated => {
          this.allTasks = this.allTasks.map(t => t.id === updated.id ? updated : t);
          this.syncTaskCounts();
          this.savingTask = false;
          this.taskVisible = false;
          this.messages.add({ severity: 'success', summary: 'Task updated', life: 2500 });
          this.cdr.detectChanges();
        },
        error: () => { this.savingTask = false; this.cdr.detectChanges(); },
      });
    } else {
      this.taskService.create(dto).subscribe({
        next: created => {
          this.allTasks = [...this.allTasks, created];
          this.syncTaskCounts();
          this.savingTask = false;
          this.taskVisible = false;
          this.messages.add({ severity: 'success', summary: 'Task added', life: 2500 });
          this.cdr.detectChanges();
        },
        error: () => { this.savingTask = false; this.cdr.detectChanges(); },
      });
    }
  }

  deleteTask(task: TodoTask, event: Event) {
    this.confirm.confirm({
      target: event.currentTarget as EventTarget,
      message: `Delete "${task.title}"? This cannot be undone.`,
      header: 'Delete Task',
      icon: 'pi pi-trash',
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      accept: () => this.removeTaskById(task.id),
    });
  }

  deleteTaskFromDialog() {
    const task = this.allTasks.find(t => t.id === this.taskForm.id);
    if (!task) return;
    this.confirm.confirm({
      message: `Delete "${task.title}"? This cannot be undone.`,
      header: 'Delete Task',
      icon: 'pi pi-trash',
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      accept: () => { this.removeTaskById(task.id); this.taskVisible = false; },
    });
  }

  private removeTaskById(id: string) {
    this.taskService.delete(id).subscribe({
      next: () => {
        this.allTasks = this.allTasks.filter(t => t.id !== id);
        this.syncTaskCounts();
        this.messages.add({ severity: 'success', summary: 'Task deleted', life: 2500 });
        this.cdr.detectChanges();
      },
    });
  }

  openNewList() {
    this.listFromTask = false;
    this.listForm = { name: '', color: '#3b82f6' };
    this.listDialogVisible = true;
  }

  openNewListFromTask() {
    this.listFromTask = true;
    this.listForm = { name: '', color: '#3b82f6' };
    this.listDialogVisible = true;
  }

  saveList() {
    if (!this.listForm.name.trim()) return;
    this.savingList = true;
    this.listService.create({ name: this.listForm.name.trim(), color: this.listForm.color }).subscribe({
      next: created => {
        const newList: TodoList = { ...created, taskCount: 0 };
        this.lists = [...this.lists, newList];
        if (this.listFromTask) {
          this.taskForm.listId = created.id;
        } else {
          this.selectedListId = created.id;
        }
        this.savingList = false;
        this.listDialogVisible = false;
        this.messages.add({ severity: 'success', summary: 'List created', life: 2500 });
        this.cdr.detectChanges();
      },
      error: () => { this.savingList = false; this.cdr.detectChanges(); },
    });
  }

  deleteList(list: TodoList, event: Event) {
    const taskCount = this.allTasks.filter(t => t.listId === list.id).length;
    const taskWarning = taskCount > 0
      ? `This will also permanently delete ${taskCount} task${taskCount === 1 ? '' : 's'} inside it.`
      : 'The list is empty.';

    this.confirm.confirm({
      target: event.currentTarget as EventTarget,
      message: `Delete list "${list.name}"? ${taskWarning}`,
      header: 'Delete List and Tasks',
      icon: 'pi pi-exclamation-triangle',
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      acceptButtonProps: { label: 'Delete All', severity: 'danger' },
      accept: () => {
        this.listService.delete(list.id).subscribe({
          next: () => {
            this.allTasks = this.allTasks.filter(t => t.listId !== list.id);
            this.lists = this.lists.filter(l => l.id !== list.id);
            if (this.selectedListId === list.id) this.selectedListId = null;
            this.messages.add({
              severity: 'success',
              summary: 'List deleted',
              detail: taskCount > 0 ? `${taskCount} task${taskCount === 1 ? '' : 's'} removed.` : undefined,
              life: 3000,
            });
            this.cdr.detectChanges();
          },
        });
      },
    });
  }
}
