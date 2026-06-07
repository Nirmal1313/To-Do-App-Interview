import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { UserService } from '../../../core/services/user.service';
import { User, UpdateUserDto } from '../../../core/models';

interface EditForm {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

@Component({
  selector: 'app-users',
  imports: [
    FormsModule,
    TableModule, ButtonModule, DialogModule,
    AvatarModule, TagModule, InputTextModule,
    SelectModule, TooltipModule, ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class UsersComponent implements OnInit {
  editVisible = false;
  editForm: EditForm = { id: '', firstName: '', lastName: '', email: '' };
  users: User[] = [];
  loading = false;

  constructor(
    private userService: UserService,
    private confirm: ConfirmationService,
    private messages: MessageService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadUsers();
  }

  private loadUsers() {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: users => { this.users = users; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); },
    });
  }

  fullName(u: User): string {
    return [u.firstName, u.lastName].filter(Boolean).join(' ');
  }

  initials(u: User): string {
    return ((u.firstName[0] ?? '') + (u.lastName?.[0] ?? '')).toUpperCase();
  }

  formatDate(iso: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  openEdit(u: User) {
    this.editForm = { id: u.id, firstName: u.firstName, lastName: u.lastName ?? '', email: u.email };
    this.editVisible = true;
  }

  saveUser() {
    const existing = this.users.find(u => u.id === this.editForm.id);
    if (!existing) return;

    const dto: UpdateUserDto = {
      id: existing.id,
      firstName: this.editForm.firstName,
      lastName: this.editForm.lastName || null,
      email: this.editForm.email,
      passwordHash: existing.passwordHash ?? '',
      isDeleted: existing.isDeleted,
      updatedBy: existing.updatedBy ?? null,
      lastUpdatedDate: new Date().toISOString(),
    };

    this.userService.update(existing.id, dto).subscribe({
      next: updated => {
        this.users = this.users.map(u => u.id === updated.id ? updated : u);
        this.editVisible = false;
        this.messages.add({ severity: 'success', summary: 'User updated', life: 2500 });
        this.cdr.detectChanges();
      },
    });
  }

  deleteUser(u: User, event: Event) {
    this.confirm.confirm({
      target: event.currentTarget as EventTarget,
      message: `Delete ${this.fullName(u)}? This cannot be undone.`,
      header: 'Delete User',
      icon: 'pi pi-trash',
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      accept: () => {
        this.userService.delete(u.id).subscribe({
          next: () => {
            this.users = this.users.filter(x => x.id !== u.id);
            this.messages.add({ severity: 'success', summary: 'User removed', life: 2500 });
            this.cdr.detectChanges();
          },
        });
      },
    });
  }
}
