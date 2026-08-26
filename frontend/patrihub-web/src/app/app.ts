import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { Auth } from './core/auth/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly auth = inject(Auth);

  protected sair(): void {
    this.auth.logout();
  }
}
