import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../modules/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const expectedRoles = route.data['roles'] as string[];
    const userRoles = this.authService.getUserRoles();

    const isAuthorized = expectedRoles?.some(role => userRoles.includes(role));

    if (!isAuthorized) {
      this.router.navigate(['/login']);
    }

    return isAuthorized;
  }
}
