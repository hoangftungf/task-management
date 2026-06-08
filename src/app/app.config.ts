import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { GoogleLoginProvider } from '@abacritt/angularx-social-login';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),

    // Đăng ký HttpClient kết hợp với cái bẫy Interceptor
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),

    //Cấu hình Google Client ID
    {
      provide: 'SocialAuthServiceConfig', // Ép kiểu chuỗi định danh chuẩn
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider("686790842399-lbmkeeegco9ing1uh0fc042iu7aadr8p.apps.googleusercontent.com")
          }
        ],
        onError: (err: any) => console.error('Social Auth Error:', err)
      }
    }
  ]
};
