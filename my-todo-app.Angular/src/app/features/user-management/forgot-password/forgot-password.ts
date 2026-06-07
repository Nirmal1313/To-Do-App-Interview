import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-forgot-password',
  imports: [ButtonModule, InputTextModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPasswordComponent {
  email = '';
  sent = false;

  // TODO: wire up password reset API
  sendReset() {
    if (!this.email.trim()) return;
    this.sent = true;
  }
}
