import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  username = '';
  password = '';
  confirmPassword = '';

  constructor(private authService: AuthService, private router: Router) {}

  onRegister(): void {
    if (this.password !== this.confirmPassword) {
      console.log('Passwords do not match');
      alert('Passwords do not match');
      return;
    }
    
    this.authService.register(this.username, this.password).subscribe({
      next: (response) => {
        console.log('Registration successful', response);
        this.authService.saveToken(response.token);
        this.authService.saveUsername(response.username);
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Registration failed', error);
        alert('Registration failed: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }
}
