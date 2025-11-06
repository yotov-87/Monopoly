import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { SignalRService } from './services/signalr.service';
import { NotificationService } from './services/notification.service';
import { AuthService } from './services/auth.service';
import { GameService } from './services/game.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  title = 'Monopoly';
  private playerJoinedSubscription?: Subscription;

  constructor(
    private signalRService: SignalRService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    console.log('App component initialized');
    console.log('Is authenticated:', this.authService.isAuthenticated());
    console.log('Username:', this.authService.getUsername());
    
    // Start SignalR connection if user is authenticated
    if (this.authService.isAuthenticated()) {
      console.log('User is authenticated, starting SignalR...');
      this.signalRService.startConnection().then(() => {
        console.log('SignalR connected successfully, joining user games...');
        
        // Join all user's games
        this.joinAllUserGames();
        
        // Listen for PlayerJoined events globally
        this.playerJoinedSubscription = this.signalRService.playerJoined$.subscribe(event => {
          console.log('PlayerJoined event received:', event);
          const currentUsername = this.authService.getUsername();
          console.log('Current username:', currentUsername);
          
          // Only show notification if the joined player is the current user
          if (event.username === currentUsername) {
            console.log('Showing invitation for:', event.gameName);
            
            // Join the game's SignalR group immediately
            this.signalRService.joinGame(event.gameId).catch(err => {
              console.error(`Failed to join game ${event.gameId}:`, err);
            });
            
            // Add notification
            this.notificationService.addInvitation({
              gameId: event.gameId,
              gameName: event.gameName,
              invitedBy: 'Game Host',
              timestamp: new Date()
            });
          }
        });
      }).catch(err => {
        console.error('Failed to start SignalR connection:', err);
      });
    } else {
      console.log('User is NOT authenticated, skipping SignalR setup');
    }
  }

  ngOnDestroy(): void {
    this.playerJoinedSubscription?.unsubscribe();
    this.signalRService.stopConnection();
  }

  private joinAllUserGames(): void {
    console.log('Fetching user games...');
    // Get all user's games and join their SignalR groups
    this.gameService.getMyGames().subscribe({
      next: (games) => {
        console.log('User games fetched:', games);
        games.forEach(game => {
          console.log(`Joining game ${game.id} (${game.name})...`);
          this.signalRService.joinGame(game.id).then(() => {
            console.log(`Successfully joined game ${game.id}`);
          }).catch(err => {
            console.error(`Failed to join game ${game.id}:`, err);
          });
        });
      },
      error: (err) => {
        console.error('Failed to fetch user games:', err);
      }
    });
  }
}