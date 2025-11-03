import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GameService, GameResponse, BoardCell, PlaygroundInfo } from '../services/game.service';
import { AuthService } from '../services/auth.service';
import { SignalRService } from '../services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-playground',
  imports: [CommonModule, FormsModule],
  templateUrl: './playground.html',
  styleUrl: './playground.scss',
})
export class PlaygroundComponent implements OnInit, OnDestroy {
  gameId: number | null = null;
  game: GameResponse | null = null;
  boardCells: BoardCell[] = [];
  loading = true;
  error: string | null = null;
  newPlayerUsername = '';
  addingPlayer = false;
  isCreator = false;
  dice1: number | null = null;
  dice2: number | null = null;
  diceTotal: number | null = null;
  currentUsername: string | null = null;
  private subscriptions: Subscription[] = [];
  hasRolledDice: boolean = false;
  hasMovedPlayer: boolean = false;
  playerReadyStates: Map<string, boolean> = new Map();
  isReady: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService,
    private authService: AuthService,
    private signalRService: SignalRService
  ) {
    this.currentUsername = this.authService.getUsername();
  }

  ngOnInit(): void {
    // Get game ID from route parameter
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.gameId = +id;
        this.loadGame();
        this.setupSignalR();
      } else {
        this.error = 'No game ID provided';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.gameId !== null) {
      this.signalRService.leaveGame(this.gameId);
    }
  }

  setupSignalR(): void {
    if (this.gameId === null) return;

    this.signalRService.startConnection().then(() => {
      if (this.gameId !== null) {
        this.signalRService.joinGame(this.gameId);
      }
    });

    // Subscribe to dice rolled events
    this.subscriptions.push(
      this.signalRService.diceRolled$.subscribe(event => {
        if (event.gameId === this.gameId && event.username !== this.currentUsername) {
          this.dice1 = event.dice1;
          this.dice2 = event.dice2;
          this.diceTotal = event.total;
        }
      })
    );

    // Subscribe to player moved events
    this.subscriptions.push(
      this.signalRService.playerMoved$.subscribe(event => {
        if (event.gameId === this.gameId) {
          console.log(`Player ${event.username} moved from ${event.fromPosition} to ${event.toPosition}`);
          
          // Reload playground data to get updated money values from server
          if (this.gameId !== null) {
            this.gameService.getPlaygroundInfo(this.gameId).subscribe({
              next: (playgroundInfo) => {
                this.boardCells = playgroundInfo.boardCells;
                // Don't update game object to avoid resetting UI state
              },
              error: (error) => {
                console.error('Failed to refresh playground data', error);
              }
            });
          }
        }
      })
    );

    // Subscribe to turn changed events
    this.subscriptions.push(
      this.signalRService.turnChanged$.subscribe(event => {
        if (event.gameId === this.gameId && this.game) {
          this.game.currentTurnUsername = event.currentTurnUsername;
          // Reset dice and move state when turn changes
          if (this.isMyTurn()) {
            // New turn for current player - reset state
            this.hasRolledDice = false;
            this.hasMovedPlayer = false;
          } else {
            // Not my turn anymore - clear dice
            this.dice1 = null;
            this.dice2 = null;
            this.diceTotal = null;
            this.hasRolledDice = false;
            this.hasMovedPlayer = false;
          }
        }
      })
    );

    // Subscribe to player ready events
    this.subscriptions.push(
      this.signalRService.playerReady$.subscribe(event => {
        if (event.gameId === this.gameId) {
          this.playerReadyStates.set(event.username, event.isReady);
          if (event.username === this.currentUsername) {
            this.isReady = event.isReady;
          }
        }
      })
    );

    // Subscribe to game started events
    this.subscriptions.push(
      this.signalRService.gameStarted$.subscribe(event => {
        if (event.gameId === this.gameId && this.game) {
          this.game.status = 'Active';
        }
      })
    );

    // Subscribe to player joined events
    this.subscriptions.push(
      this.signalRService.playerJoined$.subscribe(event => {
        if (event.gameId === this.gameId) {
          console.log(`Player ${event.username} joined the game`);
          // Reload full playground data to get updated player list
          this.loadGame();
        }
      })
    );
  }

  loadGame(): void {
    if (this.gameId === null) return;

    this.loading = true;
    this.error = null;

    this.gameService.getPlaygroundInfo(this.gameId).subscribe({
      next: (playgroundInfo) => {
        this.game = playgroundInfo.game;
        this.boardCells = playgroundInfo.boardCells;
        this.isCreator = playgroundInfo.isCreator;
        this.loading = false;

        // Initialize player positions in SignalR service
        const playerPositions = this.boardCells
          .flatMap(cell => cell.playersHere)
          .map(player => ({ username: player.username, position: player.position }));
        this.signalRService.initializePlayerPositions(playerPositions);

        // Initialize ready states from loaded data
        this.playerReadyStates.clear();
        this.boardCells
          .flatMap(cell => cell.playersHere)
          .forEach(player => {
            this.playerReadyStates.set(player.username, player.isReady || false);
            if (player.username === this.currentUsername) {
              this.isReady = player.isReady || false;
            }
          });
      },
      error: (error) => {
        console.error('Failed to load playground', error);
        if (error.status === 403) {
          this.error = 'Access denied. You are not a player in this game.';
        } else if (error.status === 401) {
          this.error = 'Please login to access this game.';
        } else {
          this.error = 'Failed to load game. It may not exist or you may not have access.';
        }
        this.loading = false;
      }
    });
  }

  onAddPlayer(): void {
    if (!this.newPlayerUsername.trim() || this.gameId === null) {
      alert('Please enter a username');
      return;
    }

    if (!this.isCreator) {
      alert('Only the game creator can add players');
      return;
    }

    this.addingPlayer = true;

    this.gameService.addPlayer(this.gameId, this.newPlayerUsername.trim()).subscribe({
      next: (response) => {
        this.game = response;
        this.newPlayerUsername = '';
        this.addingPlayer = false;
        alert(`Player added successfully!`);
      },
      error: (error) => {
        console.error('Failed to add player', error);
        alert('Failed to add player: ' + (error.error?.message || 'Unknown error'));
        this.addingPlayer = false;
      }
    });
  }

  onLeaveGame(): void {
    this.router.navigate(['/']);
  }

  onRollDice(): void {
    if (this.hasRolledDice) {
      return; // Already rolled dice this turn
    }

    this.dice1 = Math.floor(Math.random() * 6) + 1;
    this.dice2 = Math.floor(Math.random() * 6) + 1;
    this.diceTotal = this.dice1 + this.dice2;
    this.hasRolledDice = true;

    // Broadcast dice roll to other players
    if (this.gameId !== null) {
      this.gameService.rollDice(this.gameId, this.dice1, this.dice2).subscribe({
        error: (error) => {
          console.error('Failed to broadcast dice roll', error);
        }
      });

      // Move player after rolling dice
      if (this.diceTotal !== null) {
        // Get current position before move
        const currentPlayer = this.boardCells
          .flatMap(cell => cell.playersHere)
          .find(p => p.username === this.currentUsername);
        const oldPosition = currentPlayer?.position ?? 0;
        
        this.gameService.movePlayer(this.gameId, this.diceTotal).subscribe({
          next: (playgroundInfo) => {
            // Update game info AND board cells from API response
            this.game = playgroundInfo.game;
            this.boardCells = playgroundInfo.boardCells;
            this.hasMovedPlayer = true;
            
            // Check if player passed GO (position 0)
            const newPlayer = this.boardCells
              .flatMap(cell => cell.playersHere)
              .find(p => p.username === this.currentUsername);
            const newPosition = newPlayer?.position ?? 0;
            const oldMoney = currentPlayer?.money ?? 1500;
            const newMoney = newPlayer?.money ?? 1500;
            
            console.log(`Movement: oldPos=${oldPosition}, diceTotal=${this.diceTotal}, sum=${oldPosition + this.diceTotal!}, newPos=${newPosition}`);
            console.log(`Money: old=${oldMoney}, new=${newMoney}, diff=${newMoney - oldMoney}`);
            
            if (oldPosition + this.diceTotal! >= 40) {
              console.log(`✅ Passed GO! (${oldPosition} + ${this.diceTotal} >= 40)`);
              alert(`🎉 You passed GO! Collect $200`);
            } else {
              console.log(`❌ Did not pass GO (${oldPosition} + ${this.diceTotal} < 40)`);
            }
            
            // Update player positions in SignalR service
            const playerPositions = this.boardCells
              .flatMap(cell => cell.playersHere)
              .map(player => ({ username: player.username, position: player.position }));
            this.signalRService.initializePlayerPositions(playerPositions);
          },
          error: (error) => {
            console.error('Failed to move player', error);
            alert('Failed to move player: ' + (error.error?.message || 'Unknown error'));
            this.hasRolledDice = false; // Allow re-roll on error
          }
        });
      }
    }
  }

  onEndTurn(): void {
    if (this.gameId === null) return;

    this.gameService.endTurn(this.gameId).subscribe({
      next: (response) => {
        this.game = response;
        this.dice1 = null;
        this.dice2 = null;
        this.diceTotal = null;
        this.hasRolledDice = false;
        this.hasMovedPlayer = false;
      },
      error: (error) => {
        console.error('Failed to end turn', error);
        alert('Failed to end turn: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  isMyTurn(): boolean {
    return this.game?.currentTurnUsername === this.currentUsername;
  }

  canRollDice(): boolean {
    return this.isMyTurn() && !this.hasRolledDice && this.game?.status === 'Active';
  }

  canEndTurn(): boolean {
    return this.isMyTurn() && this.hasRolledDice && this.hasMovedPlayer;
  }

  onToggleReady(): void {
    if (this.gameId === null) return;

    const newReadyState = !this.isReady;
    
    this.gameService.setPlayerReady(this.gameId, newReadyState).subscribe({
      next: () => {
        console.log(`Ready status set to ${newReadyState}`);
      },
      error: (error) => {
        console.error('Failed to set ready status', error);
        alert('Failed to set ready status: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  isPlayerReady(username: string): boolean {
    return this.playerReadyStates.get(username) || false;
  }

  updateBoardCellsWithPlayerPosition(username: string, fromPosition: number, toPosition: number): void {
    // Remove player from old position
    const fromCell = this.boardCells.find(cell => cell.position === fromPosition);
    let savedPlayerInfo = null;
    
    if (fromCell) {
      savedPlayerInfo = fromCell.playersHere.find(p => p.username === username);
      fromCell.playersHere = fromCell.playersHere.filter(p => p.username !== username);
    }

    // Add player to new position
    const toCell = this.boardCells.find(cell => cell.position === toPosition);
    if (toCell) {
      const existingPlayer = toCell.playersHere.find(p => p.username === username);
      if (!existingPlayer) {
        // Use saved player info to preserve money
        const playerInfo = savedPlayerInfo || {
          username: username,
          position: toPosition,
          money: 1500, // Default value
          color: this.getPlayerColor(username),
          isReady: false // Default to not ready
        };
        
        // Check if player passed GO (when moving from higher position to lower, or distance >= 40)
        const didPassGo = fromPosition > toPosition || 
                         (toPosition - fromPosition) < 0 ||
                         (fromPosition + (toPosition >= fromPosition ? toPosition - fromPosition : 40 - fromPosition + toPosition)) >= 40;
        
        if (didPassGo && savedPlayerInfo) {
          playerInfo.money += 200;
          console.log(`${username} passed GO! +$200, new balance: $${playerInfo.money}`);
        }
        
        playerInfo.position = toPosition;
        toCell.playersHere.push(playerInfo);
      } else {
        // Update existing player position
        existingPlayer.position = toPosition;
      }
    }
  }

  updatePlayerMoney(username: string, newMoney: number): void {
    // Update money for player in all cells
    this.boardCells.forEach(cell => {
      cell.playersHere.forEach(player => {
        if (player.username === username) {
          player.money = newMoney;
        }
      });
    });
  }

  getPlayerColor(username: string): string {
    // Find player color from existing players
    for (const cell of this.boardCells) {
      const player = cell.playersHere.find(p => p.username === username);
      if (player) {
        return player.color;
      }
    }
    // Default colors if not found
    const colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E2'];
    const playerIndex = this.game?.players.indexOf(username) ?? 0;
    return colors[playerIndex % colors.length];
  }

  getBoardCell(position: number): BoardCell | undefined {
    return this.boardCells.find(cell => cell.position === position);
  }

  getAllPlayers(): Array<{username: string, color: string, money: number, currentPosition: number}> {
    const uniquePlayers = new Map<string, {username: string, color: string, money: number, currentPosition: number}>();
    
    this.boardCells.forEach(cell => {
      cell.playersHere.forEach(player => {
        if (!uniquePlayers.has(player.username)) {
          uniquePlayers.set(player.username, {
            username: player.username,
            color: player.color,
            money: player.money,
            currentPosition: player.position
          });
        }
      });
    });
    
    return Array.from(uniquePlayers.values());
  }

  cellHasCurrentPlayer(position: number): boolean {
    const cell = this.getBoardCell(position);
    if (!cell || !this.game?.currentTurnUsername) {
      return false;
    }
    return cell.playersHere.some(p => p.username === this.game?.currentTurnUsername);
  }

  getCellPlayerColors(position: number): string[] {
    const cell = this.getBoardCell(position);
    if (!cell || cell.playersHere.length === 0) {
      return [];
    }
    return cell.playersHere.map(p => p.color);
  }

  getCellBorderStyle(position: number): any {
    const colors = this.getCellPlayerColors(position);
    if (colors.length === 0) {
      return {};
    }
    
    if (colors.length === 1) {
      return {
        'border-color': colors[0],
        'border-width': '4px',
        'box-shadow': `0 0 15px ${colors[0]}80`
      };
    }
    
    // Multiple players - create gradient border effect
    const gradient = `linear-gradient(135deg, ${colors.join(', ')})`;
    return {
      'border-image': `${gradient} 1`,
      'border-width': '4px',
      'box-shadow': `0 0 15px ${colors[0]}60`
    };
  }
}
