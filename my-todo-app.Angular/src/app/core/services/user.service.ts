import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { User, UpdateUserDto } from '../models';

export type { User, UpdateUserDto };

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly base = `${environment.apiUrl}/Users`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<User[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<User>(`${this.base}/${id}`);
  }

  emailExists(email: string) {
    return this.http.get<boolean>(`${this.base}/email/${email}`);
  }

  update(id: string, dto: UpdateUserDto) {
    return this.http.put<User>(`${this.base}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
