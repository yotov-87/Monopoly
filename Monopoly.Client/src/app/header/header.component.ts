import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { NotificationService, GameInvitation } from '../services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  invitations: GameInvitation[] = [];
  private invitationsSubscription?: Subscription;

  constructor(
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    console.log('Header component initialized');
    // Subscribe to invitations
    this.invitationsSubscription = this.notificationService.invitations$.subscribe(
      invitations => {
        console.log('Header received invitations update:', invitations);
        console.log('Invitations length:', invitations.length);
        this.invitations = invitations;
        console.log('Header invitations array:', this.invitations);
      }
    );
  }

  ngOnDestroy(): void {
    this.invitationsSubscription?.unsubscribe();
  }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  get username(): string | null {
    return this.authService.getUsername();
  }

  onLogin(): void {
    this.router.navigate(['/login']);
  }

  onRegister(): void {
    this.router.navigate(['/register']);
  }

  onLogout(): void {
    this.authService.logout();
    this.notificationService.clearAllInvitations();
    this.router.navigate(['/']);
  }

  onCreateGame(): void {
    this.router.navigate(['/create-game']);
  }

  onMyGames(): void {
    this.router.navigate(['/my-games']);
  }

  onJoinGame(invitation: GameInvitation): void {
    this.notificationService.removeInvitation(invitation.gameId);
    this.router.navigate(['/playground', invitation.gameId]);
  }

  onDismissInvitation(invitation: GameInvitation, event: Event): void {
    event.stopPropagation();
    this.notificationService.removeInvitation(invitation.gameId);
  }
}
