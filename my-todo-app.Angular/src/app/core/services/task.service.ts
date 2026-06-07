import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TodoTask, TaskDto, TaskStatus, TaskPriority } from '../models';

export type { TodoTask, TaskDto, TaskStatus, TaskPriority };

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly base = `${environment.apiUrl}/ToDoTask`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<TodoTask[]>(`${this.base}/all`);
  }

  getById(id: string) {
    return this.http.get<TodoTask>(`${this.base}/${id}`);
  }

  getByStatus(status: TaskStatus) {
    return this.http.get<TodoTask[]>(`${this.base}/status`, { params: { status } });
  }

  getByPriority(priority: TaskPriority) {
    return this.http.get<TodoTask[]>(`${this.base}/priority`, { params: { priority } });
  }

  getByTitle(title: string) {
    return this.http.get<TodoTask[]>(`${this.base}/title`, { params: { title } });
  }

  getByList(listId: string) {
    return this.http.get<TodoTask[]>(`${this.base}/list/${listId}`);
  }

  create(dto: TaskDto) {
    return this.http.post<TodoTask>(this.base, dto);
  }

  update(id: string, dto: TaskDto) {
    return this.http.put<TodoTask>(`${this.base}/${id}`, dto);
  }

  updateTags(id: string, tagIds: string[]) {
    return this.http.put<void>(`${this.base}/${id}/tags`, tagIds);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
