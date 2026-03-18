import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

/**
 * Login Page - Matches backend API:
 * POST /api/users/login with { email, password }
 * Returns: { token, userId, email, role }
 * Errors: 401 - "Invalid username or password" or "Please verify your email first"
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  forgotEmail: string = '';
  newPassword: string = '';
 // errorMessage: string = '';
  successMessage: string = '';
  isForgotPasswordMode: boolean = false;
  isResetPasswordStep: boolean = false;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private authService: AuthService
  ) {}

  onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.email?.trim()) {
      this.errorMessage = 'Please enter your email';
      return;
    }

    if (!this.password?.trim()) {
      this.errorMessage = 'Please enter your password';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) {
      this.errorMessage = 'Please enter a valid email address';
      return;
    }

    this.isLoading = true;

    this.apiService.login({
      email: this.email.trim().toLowerCase(),
      password: this.password
    }).subscribe({
      next: (response) => {
        this.authService.setSession(response.token, response.email, response.role,Number(response.userId),response.name);
        this.isLoading = false;

        if (response.role === 'Manager') {
          this.router.navigate(['/manager-dashboard']);
        } else if (response.role === 'Employee') {
          this.router.navigate(['/employee-dashboard']);
        } else if (response.role === 'Admin') {
          this.router.navigate(['/admin-dashboard']);
        }
      },
      error: (error) => {
        this.errorMessage = this.apiService.getErrorMessage(error, 'Invalid email or password');
        this.isLoading = false;
      }
    });
  }
  startForgotPassword() {

this.errorMessage = '';

this.successMessage = '';

this.isForgotPasswordMode = true;

this.isResetPasswordStep = false;

this.forgotEmail = this.email?.trim().toLowerCase() ?? '';

this.newPassword = '';

}
 
backToLogin(clearMessages: boolean = true) {

if (clearMessages) {

this.errorMessage = '';

this.successMessage = '';

}

this.isForgotPasswordMode = false;

this.isResetPasswordStep = false;

this.forgotEmail = '';

this.newPassword = '';

}
 
submitForgotEmail() {

this.errorMessage = '';

this.successMessage = '';
 
if (!this.forgotEmail?.trim()) {

this.errorMessage = 'Please enter your email';

return;

}
 
this.isLoading = true;

this.apiService.validateForgotPasswordEmail({

email: this.forgotEmail.trim().toLowerCase()

}).subscribe({

next: () => {

this.isResetPasswordStep = true;

this.isLoading = false;

},

error: (error) => {

if (error?.status === 404) {

this.errorMessage = 'Forgot password service is not available right now. Please restart backend and try again.';

} else {

this.errorMessage = this.apiService.getErrorMessage(error, 'Invalid email');

}

this.isLoading = false;

}

});

}
 
submitNewPassword() {

this.errorMessage = '';

this.successMessage = '';
 
if (!this.newPassword?.trim()) {

this.errorMessage = 'Please enter a new password';

return;

}
 
this.isLoading = true;

this.apiService.resetPassword({

email: this.forgotEmail.trim().toLowerCase(),

newPassword: this.newPassword

}).subscribe({

next: () => {

this.successMessage = 'Password updated successfully. Please sign in.';

this.isLoading = false;

this.backToLogin(false);

},

error: (error) => {

if (error?.status === 404) {

this.errorMessage = 'Forgot password service is not available right now. Please restart backend and try again.';

} else {

this.errorMessage = this.apiService.getErrorMessage(error, 'Unable to update password');

}

this.isLoading = false;

}

});

}

 

  navigateToRegister() {
    this.router.navigate(['/register']);
  }

  navigateToHome() {
    this.router.navigate(['/']);
  }
}
