import { ApplicationConfig, ErrorHandler, provideZoneChangeDetection } from '@angular/core';

import { provideAnimations } from '@angular/platform-browser/animations';

import { provideHttpClient, withInterceptors, withXsrfConfiguration } from '@angular/common/http';

import { provideRouter } from '@angular/router';

import { provideToastr } from 'ngx-toastr';



import { routes } from './app.routes';

import { authInterceptor } from './core/interceptors/auth.interceptor';

import { credentialsInterceptor } from './core/interceptors/credentials.interceptor';

import { httpErrorInterceptor } from './core/interceptors/http-error.interceptor';

import { GlobalErrorHandler } from './core/errors/global-error.handler';



export const appConfig: ApplicationConfig = {

  providers: [

    provideZoneChangeDetection({ eventCoalescing: true }),

    provideAnimations(),

    provideRouter(routes),

    provideHttpClient(

      withXsrfConfiguration({

        cookieName: 'XSRF-TOKEN',

        headerName: 'X-XSRF-TOKEN',

      }),

      withInterceptors([credentialsInterceptor, authInterceptor, httpErrorInterceptor])

    ),

    { provide: ErrorHandler, useClass: GlobalErrorHandler },

    provideToastr({

      timeOut: 3500,

      positionClass: 'toast-top-right',

      preventDuplicates: true,

    }),

  ],

};


