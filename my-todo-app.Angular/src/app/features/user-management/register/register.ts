import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ButtonModule, InputTextModule, PasswordModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class RegisterComponent {
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private messages: MessageService,
  ) {}

  register() {
    if (!this.firstName.trim() || !this.lastName.trim() || !this.email.trim() || !this.password) return;
    if (this.password !== this.confirmPassword) {
      this.messages.add({ severity: 'warn', summary: 'Passwords do not match', life: 3000 });
      return;
    }
    this.loading = true;
    this.auth.register({
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password,
      confirmPassword: this.confirmPassword,
    }).subscribe({
      next: () => {
        this.messages.add({ severity: 'success', summary: 'Account created', detail: 'You can now sign in', life: 3000 });
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: () => { this.loading = false; },
    });
  }
}
