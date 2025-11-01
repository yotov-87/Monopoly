import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GameService } from '../services/game.service';

@Component({
  selector: 'app-create-game',
  imports: [FormsModule, CommonModule],
  templateUrl: './create-game.html',
  styleUrl: './create-game.scss'
})
export class CreateGameComponent {
  gameName = '';
  playerCount = 2;
  minPlayers = 2;
  maxPlayers = 8;

  constructor(private router: Router, private gameService: GameService) {}

  onCreateGame(): void {
    if (!this.gameName.trim()) {
      alert('Please enter a game name');
      return;
    }

    if (this.playerCount < this.minPlayers || this.playerCount > this.maxPlayers) {
      alert(`Player count must be between ${this.minPlayers} and ${this.maxPlayers}`);
      return;
    }

    this.gameService.createGame({
      name: this.gameName,
      playerCount: this.playerCount
    }).subscribe({
      next: (response) => {
        console.log('Game created successfully', response);
        alert(`Game "${response.name}" created successfully!`);
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Failed to create game', error);
        alert('Failed to create game: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }
}
