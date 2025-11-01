import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  username = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  onLogin(): void {
    this.authService.login(this.username, this.password).subscribe({
      next: (response) => {
        console.log('Login successful', response);
        this.authService.saveToken(response.token);
        this.authService.saveUsername(response.username);
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Login failed', error);
        alert('Login failed: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }
}
