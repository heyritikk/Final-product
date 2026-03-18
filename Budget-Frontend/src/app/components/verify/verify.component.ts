import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';

/**
 * Verify Page - Handles email verification from registration link
 * GET /api/users/verify?token=xxx
 * Called when user clicks verification link in email
 */
@Component({
  selector: 'app-verify',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './verify.component.html',
  styleUrl: './verify.component.css'
})
export class VerifyComponent implements OnInit {
  status: 'loading' | 'success' | 'error' = 'loading';
  message: string = '';
  errorDetails: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService
  ) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.status = 'error';
      this.message = 'Verification failed';
      this.errorDetails = 'No verification token provided. Please use the link from your email.';
      return;
    }

    this.apiService.verifyEmail(token).subscribe({
      next: (response) => {
        this.status = 'success';
        this.message = typeof response === 'string' ? response : 'Successfully verified';
        this.errorDetails = '';
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (error) => {
        this.status = 'error';
        this.message = 'Verification failed';
        this.errorDetails = this.apiService.getErrorMessage(error, 'Invalid or expired token.');
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToHome() {
    this.router.navigate(['/']);
  }
}
