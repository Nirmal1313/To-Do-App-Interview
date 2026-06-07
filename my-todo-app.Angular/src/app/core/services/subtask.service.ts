import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { SubTask, SubTaskDto } from '../models';

export type { SubTaskDto };

@Injectable({ providedIn: 'root' })
export class SubtaskService {
  private readonly base = `${environment.apiUrl}/SubTask`;

  constructor(private http: HttpClient) {}

  getByTask(taskId: string) {
    return this.http.get<SubTask[]>(`${this.base}/task/${taskId}`);
  }

  create(dto: SubTaskDto) {
    return this.http.post<SubTask>(this.base, dto);
  }

  update(id: string, dto: SubTaskDto) {
    return this.http.put<SubTask>(`${this.base}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
