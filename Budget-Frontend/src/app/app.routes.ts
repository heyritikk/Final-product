import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { RegisterComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { VerifyComponent } from './components/verify/verify.component';
import { ManagerDashboardComponent } from './components/manager-dashboard/manager-dashboard.component';
import { EmployeeDashboardComponent } from './components/employee-dashboard/employee-dashboard.component';
import { authGuard } from './guards/auth.guard';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
 
export const routes: Routes = [
 
  { path: '', component: LandingComponent },
 
  { path: 'register', component: RegisterComponent },
 
  { path: 'login', component: LoginComponent },
 
  { path: 'verify', component: VerifyComponent },
 
  { path: 'manager-dashboard', component: ManagerDashboardComponent, canActivate:[authGuard], data:{ roles:['Manager'] } },
 
  { path: 'employee-dashboard', component: EmployeeDashboardComponent, canActivate:[authGuard], data:{ roles:['Employee'] } },

  { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate:[authGuard], data:{ roles:['Admin'] } },
 
  // ** wildcard route to catch undefined paths and redirect to landing page
  { path: '**', redirectTo: '' }
 
];
 