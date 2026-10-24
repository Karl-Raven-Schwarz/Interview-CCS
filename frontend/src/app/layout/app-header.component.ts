import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Subscription } from 'rxjs';

import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './app-header.component.html'
})
export class AppHeaderComponent implements OnInit, OnDestroy {
  readonly title = signal('Entidades');
  readonly menuOpen = signal(false);

  private subscription = new Subscription();

  constructor(
    private router: Router,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.subscription.add(
      this.router.events
        .pipe(filter((event) => event instanceof NavigationEnd))
        .subscribe(() => this.title.set(this.labelFor(this.router.url)))
    );
    this.title.set(this.labelFor(this.router.url));
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  get initials(): string {
    return (this.auth.user()?.username ?? '?').slice(0, 2).toUpperCase();
  }

  logout(): void {
    this.menuOpen.set(false);
    this.auth.logout();
  }

  private labelFor(url: string): string {
    if (url.startsWith('/entities/new')) {
      return 'Nueva entidad';
    }
    if (/^\/entities\/\d+\/edit/.test(url)) {
      return 'Editar entidad';
    }
    return 'Entidades';
  }
}
