import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TodoList, ToDoListDto } from '../models';

export type { TodoList, ToDoListDto };

@Injectable({ providedIn: 'root' })
export class ListService {
  private readonly base = `${environment.apiUrl}/ToDoList`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<TodoList[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<TodoList>(`${this.base}/${id}`);
  }

  create(dto: ToDoListDto) {
    return this.http.post<TodoList>(this.base, dto);
  }

  update(id: string, dto: ToDoListDto) {
    return this.http.put<TodoList>(`${this.base}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
