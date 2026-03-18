import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const allowedRoles = route.data?.['roles'] as string[] | undefined;
  // if route does not specify allowedRoles,allow Access.
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const userRole = authService.getRole();

  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  // User is logged in but does not have the required role
  if (userRole === 'Manager') {
    router.navigate(['/manager-dashboard']);
  } else if (userRole === 'Employee') {
    router.navigate(['/employee-dashboard']);
  } else {
    router.navigate(['/']);
  }

  return false;
};
