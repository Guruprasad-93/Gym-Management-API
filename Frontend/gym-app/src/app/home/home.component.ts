import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  pageHtml = '';

  // We copy the template folder into `dist/gym-app/ahana/*` via angular.json.
  private readonly assetBaseUrl = '/ahana';

  private readonly templateCssFiles: string[] = [
    `${this.assetBaseUrl}/css/bootstrap.min.css`,
    `${this.assetBaseUrl}/css/font-awesome.min.css`,
    `${this.assetBaseUrl}/css/owl.carousel.min.css`,
    `${this.assetBaseUrl}/css/nice-select.css`,
    `${this.assetBaseUrl}/css/magnific-popup.css`,
    `${this.assetBaseUrl}/css/slicknav.min.css`,
    `${this.assetBaseUrl}/css/animate.css`,
    `${this.assetBaseUrl}/css/style.css`,
  ];

  private readonly templateScriptFiles: string[] = [
    `${this.assetBaseUrl}/js/vendor/jquery-3.2.1.min.js`,
    `${this.assetBaseUrl}/js/bootstrap.min.js`,
    `${this.assetBaseUrl}/js/jquery.slicknav.min.js`,
    `${this.assetBaseUrl}/js/owl.carousel.min.js`,
    `${this.assetBaseUrl}/js/jquery.nice-select.min.js`,
    `${this.assetBaseUrl}/js/jquery-ui.min.js`,
    `${this.assetBaseUrl}/js/jquery.magnific-popup.min.js`,
    `${this.assetBaseUrl}/js/main.js`,
  ];

  private static cssLoaded = false;
  private static scriptsLoaded = false;

  ngOnInit(): void {
    void this.init();
  }

  private async init(): Promise<void> {
    await this.ensureCssLoaded();
    await this.loadAndRenderTemplateBody();
    await this.ensureScriptsLoaded();
  }

  private async ensureCssLoaded(): Promise<void> {
    if (HomeComponent.cssLoaded) return;

    await Promise.all(
      this.templateCssFiles.map(async (href) => {
        if (document.querySelector(`link[rel="stylesheet"][href="${href}"]`)) {
          return;
        }

        await new Promise<void>((resolve) => {
          const link = document.createElement('link');
          link.rel = 'stylesheet';
          link.href = href;
          link.onload = () => resolve();
          link.onerror = () => resolve(); // avoid blocking rendering if a request fails
          document.head.appendChild(link);
        });
      })
    );

    HomeComponent.cssLoaded = true;
  }

  private async ensureScriptsLoaded(): Promise<void> {
    if (HomeComponent.scriptsLoaded) return;

    // Load in order (main.js depends on vendor libraries).
    for (const src of this.templateScriptFiles) {
      if (document.querySelector(`script[src="${src}"]`)) {
        continue;
      }

      await new Promise<void>((resolve) => {
        const script = document.createElement('script');
        script.src = src;
        script.async = false;
        script.onload = () => resolve();
        script.onerror = () => resolve(); // don't hard-fail homepage rendering
        document.body.appendChild(script);
      });
    }

    HomeComponent.scriptsLoaded = true;
  }

  private async loadAndRenderTemplateBody(): Promise<void> {
    const response = await fetch(`${this.assetBaseUrl}/index.html`);
    const html = await response.text();

    const doc = new DOMParser().parseFromString(html, 'text/html');
    const body = doc.body;

    // Replace the inline year script used by the template footer.
    const year = new Date().getFullYear().toString();

    // Remove external script tags (we load the scripts explicitly above).
    body.querySelectorAll('script[src]').forEach((s) => s.remove());

    // Replace inline scripts that generate the current year.
    body.querySelectorAll('script:not([src])').forEach((s) => {
      const text = s.textContent ?? '';
      if (text.includes('getFullYear')) {
        s.replaceWith(year);
      } else {
        s.remove();
      }
    });

    // Rewrite asset paths so that images/backgrounds load from `/ahana/*`.
    body.querySelectorAll('[src]').forEach((el) => {
      const src = el.getAttribute('src');
      if (src && src.startsWith('img/')) {
        el.setAttribute('src', `${this.assetBaseUrl}/${src}`);
      }
    });

    body.querySelectorAll('[data-setbg]').forEach((el) => {
      const value = el.getAttribute('data-setbg');
      if (value && value.startsWith('img/')) {
        el.setAttribute('data-setbg', `${this.assetBaseUrl}/${value}`);
      }
    });

    // Add "Login" link directly after the existing "Contact" link (keep template feel).
    this.insertLoginLinkNextToContact(doc);

    // Add "Register" link after the injected "Login" item.
    this.insertRegisterLinkNextToLogin(doc);

    // Render the template body inside this component.
    this.pageHtml = body.innerHTML;
  }

  private insertLoginLinkNextToContact(doc: Document): void {
    const menu = doc.querySelector('ul.main-menu');
    if (!menu) return;

    const contactAnchor = menu.querySelector<HTMLAnchorElement>('a[href*="contact"]');
    const contactText = (contactAnchor?.textContent ?? '').trim().toLowerCase();
    if (!contactAnchor || contactText !== 'contact') return;

    // Don't duplicate if user navigates back to home.
    if (menu.querySelector('a[href="/login"]')) return;

    const contactLi = contactAnchor.closest('li');
    if (!contactLi || !contactLi.parentElement) return;

    const loginLi = doc.createElement('li');
    const loginAnchor = doc.createElement('a');
    loginAnchor.setAttribute('href', '/login');
    loginAnchor.textContent = 'Login';
    loginLi.appendChild(loginAnchor);

    // Insert right after the contact item.
    contactLi.parentElement.insertBefore(loginLi, contactLi.nextSibling);
  }

  private insertRegisterLinkNextToLogin(doc: Document): void {
    const menu = doc.querySelector('ul.main-menu');
    if (!menu) return;

    if (menu.querySelector('a[href="/register"]')) return;

    const loginAnchor = menu.querySelector<HTMLAnchorElement>('a[href="/login"], a[href*="/login"]');
    if (!loginAnchor) return;

    const loginLi = loginAnchor.closest('li');
    if (!loginLi || !loginLi.parentElement) return;

    const registerLi = doc.createElement('li');
    const registerAnchor = doc.createElement('a');
    registerAnchor.setAttribute('href', '/register');
    registerAnchor.textContent = 'Register';
    registerLi.appendChild(registerAnchor);

    loginLi.parentElement.insertBefore(registerLi, loginLi.nextSibling);
  }
}

