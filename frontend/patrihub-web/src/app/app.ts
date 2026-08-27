import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { Auth } from './core/auth/auth';
import { Tema } from './core/tema/tema';

const CHAVE_SIDEBAR_COLAPSADA = 'patrihub.sidebarColapsada';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly tema = inject(Tema);

  protected readonly sidebarColapsada = signal(localStorage.getItem(CHAVE_SIDEBAR_COLAPSADA) === 'true');

  protected alternarSidebar(): void {
    const colapsada = !this.sidebarColapsada();
    this.sidebarColapsada.set(colapsada);
    localStorage.setItem(CHAVE_SIDEBAR_COLAPSADA, String(colapsada));
  }

  protected sair(): void {
    this.auth.logout();
  }
}
