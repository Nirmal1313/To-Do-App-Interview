import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TagDto } from '../models';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly base = `${environment.apiUrl}/Tag`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<TagDto[]>(this.base);
  }

  create(dto: TagDto) {
    return this.http.post<TagDto>(this.base, dto);
  }

  assignToTask(tagId: string, taskId: string) {
    return this.http.post<void>(`${this.base}/${tagId}/task/${taskId}`, {});
  }

  removeFromTask(tagId: string, taskId: string) {
    return this.http.delete<void>(`${this.base}/${tagId}/task/${taskId}`);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
