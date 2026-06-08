import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="app-footer">
      <span>&copy; {{ year }} Gym Management SaaS</span>
    </footer>
  `,
  styles: [
    `
      .app-footer {
        padding: 0.75rem 1.5rem;
        text-align: center;
        font-size: 0.85rem;
        color: #666;
        border-top: 1px solid #e0e0e0;
        background: #fff;
      }
    `,
  ],
})
export class FooterComponent {
  year = new Date().getFullYear();
}
