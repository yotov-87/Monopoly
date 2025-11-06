import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GameService } from '../services/game.service';

interface MyGame {
  id: number;
  name: string;
  status: string;
  playerCount: number;
  currentPlayerCount: number;
  createdAt: string;
}

@Component({
  selector: 'app-my-games',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-games.component.html',
  styleUrls: ['./my-games.component.scss']
})
export class MyGamesComponent implements OnInit {
  games: MyGame[] = [];
  loading = true;
  error: string | null = null;

  constructor(
    private router: Router,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    this.loadMyGames();
  }

  loadMyGames(): void {
    this.loading = true;
    this.error = null;

    this.gameService.getMyGames().subscribe({
      next: (games) => {
        this.games = games;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load games', error);
        this.error = 'Failed to load your games. Please try again.';
        this.loading = false;
      }
    });
  }

  onJoinGame(gameId: number): void {
    this.router.navigate(['/playground', gameId]);
  }

  onCreateNewGame(): void {
    this.router.navigate(['/create-game']);
  }

  onRefresh(): void {
    this.loadMyGames();
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'waiting':
        return 'status-waiting';
      case 'inprogress':
        return 'status-active';
      case 'finished':
        return 'status-finished';
      default:
        return '';
    }
  }

  getStatusLabel(status: string): string {
    switch (status.toLowerCase()) {
      case 'waiting':
        return 'Waiting for Players';
      case 'inprogress':
        return 'In Progress';
      case 'finished':
        return 'Finished';
      default:
        return status;
    }
  }
}
