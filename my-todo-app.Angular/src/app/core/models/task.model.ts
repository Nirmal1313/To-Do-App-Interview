import { TagDto } from './tag.model';
import { SubTask } from './subtask.model';

export type TaskStatus = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export type TaskPriority = 1 | 2 | 3 | 4;

export interface TodoTask {
  id: string;
  title: string;
  description: string;
  status: TaskStatus | null;
  priority: TaskPriority | null;
  createdDate: string | null;
  dueDate: string | null;
  isCompleted: boolean;
  assignedTo: string | null;
  createdBy: string | null;
  updatedBy: string | null;
  listId: string | null;
  tags: TagDto[];
  subTasks: SubTask[];
}

export interface TaskDto {
  id?: string | null;
  title: string;
  description: string;
  status?: TaskStatus | null;
  priority?: TaskPriority | null;
  createdDate?: string | null;
  dueDate?: string | null;
  isCompleted?: boolean;
  assignedTo?: string | null;
  createdBy?: string | null;
  updatedBy?: string | null;
  listId?: string | null;
  tags?: TagDto[];
  subTasks?: SubTask[];
}
