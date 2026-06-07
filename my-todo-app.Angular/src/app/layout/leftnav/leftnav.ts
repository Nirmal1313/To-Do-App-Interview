import { Component, Input, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-leftnav',
  imports: [RouterLink, RouterLinkActive, AvatarModule, TooltipModule],
  templateUrl: './leftnav.html',
  styleUrl: './leftnav.scss',
})
export class LeftnavComponent {
  @Input() collapsed = false;

  private readonly auth = inject(AuthService);
  readonly userEmail = this.auth.currentUserEmail;

  navItems = [
    { label: 'Dashboard', icon: 'pi pi-home',         route: '/dashboard' },
    { label: 'My Tasks',  icon: 'pi pi-check-square', route: '/tasks' },
    { label: 'Users',     icon: 'pi pi-users',        route: '/users' },
  ];

  get userInitial(): string {
    return this.userEmail()?.charAt(0).toUpperCase() ?? 'U';
  }

  logout() {
    this.auth.logout();
  }
}
