import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
 
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
 
  role: string = '';
  userName: string = '';
 
  constructor(
    private auth: AuthService,
    private router: Router
  ) {
    this.role = this.auth.getRole() ?? '';
    this.userName = localStorage.getItem('name')?? '';
  }
 
  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
 