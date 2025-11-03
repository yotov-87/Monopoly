import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { Subject, BehaviorSubject } from 'rxjs';

export interface DiceRolledEvent {
  gameId: number;
  username: string;
  dice1: number;
  dice2: number;
  total: number;
}

export interface PlayerMovedEvent {
  gameId: number;
  username: string;
  fromPosition: number;
  toPosition: number;
}

export interface TurnChangedEvent {
  gameId: number;
  currentTurnUsername: string;
  previousTurnUsername: string;
}

export interface PlayerPositionUpdate {
  username: string;
  position: number;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private hubUrl = 'http://localhost:5262/hubs/game';

  public diceRolled$ = new Subject<DiceRolledEvent>();
  public playerMoved$ = new Subject<PlayerMovedEvent>();
  public turnChanged$ = new Subject<TurnChangedEvent>();
  public playerPositions$ = new BehaviorSubject<Map<string, number>>(new Map());

  constructor(private authService: AuthService) {}

  public startConnection(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();

    return this.hubConnection
      .start()
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.error('Error starting SignalR connection:', err));
  }

  public stopConnection(): Promise<void> {
    if (this.hubConnection) {
      return this.hubConnection.stop();
    }
    return Promise.resolve();
  }

  public joinGame(gameId: number): Promise<void> {
    if (this.hubConnection) {
      return this.hubConnection.invoke('JoinGame', gameId);
    }
    return Promise.reject('No connection established');
  }

  public leaveGame(gameId: number): Promise<void> {
    if (this.hubConnection) {
      return this.hubConnection.invoke('LeaveGame', gameId);
    }
    return Promise.reject('No connection established');
  }

  public updatePlayerPosition(username: string, position: number): void {
    const positions = this.playerPositions$.value;
    positions.set(username, position);
    this.playerPositions$.next(new Map(positions));
  }

  public initializePlayerPositions(players: { username: string; position: number }[]): void {
    const positions = new Map<string, number>();
    players.forEach(player => {
      positions.set(player.username, player.position);
    });
    this.playerPositions$.next(positions);
  }

  public getPlayerPosition(username: string): number | undefined {
    return this.playerPositions$.value.get(username);
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('DiceRolled', (event: DiceRolledEvent) => {
      this.diceRolled$.next(event);
    });

    this.hubConnection.on('PlayerMoved', (event: PlayerMovedEvent) => {
      this.updatePlayerPosition(event.username, event.toPosition);
      this.playerMoved$.next(event);
    });

    this.hubConnection.on('TurnChanged', (event: TurnChangedEvent) => {
      this.turnChanged$.next(event);
    });
  }
}
