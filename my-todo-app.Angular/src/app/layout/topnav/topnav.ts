import { Component, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { AvatarModule } from 'primeng/avatar';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { ToolbarModule } from 'primeng/toolbar';
import { FooterComponent } from '../footer/footer';
import { LeftnavComponent } from '../leftnav/leftnav';

@Component({
  selector: 'app-topnav',
  imports: [RouterOutlet, ButtonModule, AvatarModule, ToolbarModule,
            BreadcrumbModule, LeftnavComponent, FooterComponent],
  templateUrl: './topnav.html',
  styleUrl: './topnav.scss',
})
export class TopnavComponent {
  collapsed = signal(false);

  homeItem: MenuItem = { icon: 'pi pi-home', routerLink: '/dashboard' };
  breadcrumbItems: MenuItem[] = [];

  private readonly routeLabels: Record<string, string> = {
    dashboard: 'Dashboard',
    users: 'Users',
    tasks: 'My Tasks',
    settings: 'Settings',
  };

  constructor(private router: Router) {
    this.buildBreadcrumb();
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.buildBreadcrumb());
  }

  private buildBreadcrumb() {
    const segments = this.router.url.split('?')[0].split('/').filter(Boolean);
    this.breadcrumbItems = segments
      .filter(s => this.routeLabels[s])
      .map(s => ({ label: this.routeLabels[s] }));
  }

  toggleSidebar() {
    this.collapsed.update(v => !v);
  }
}
