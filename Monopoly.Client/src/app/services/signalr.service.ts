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

export interface PlayerReadyEvent {
  gameId: number;
  username: string;
  isReady: boolean;
  allPlayersReady: boolean;
}

export interface GameStartedEvent {
  gameId: number;
}

export interface PlayerJoinedEvent {
  gameId: number;
  gameName: string;
  username: string;
  currentPlayerCount: number;
  maxPlayerCount: number;
}

export interface RentPaidEvent {
  gameId: number;
  cellId: number;
  tenantUsername: string;
  ownerUsername: string;
  amount: number;
  isMonopoly: boolean;
}

export interface TradeProposedEvent {
  tradeId: number;
  gameId: number;
  buyerUsername: string;
  sellerUsername: string;
  cellPosition: number;
  cellName: string;
  offeredPrice: number;
}

export interface TradeAcceptedEvent {
  tradeId: number;
  gameId: number;
  buyerUsername: string;
  sellerUsername: string;
  cellPosition: number;
  cellName: string;
  price: number;
}

export interface TradeRejectedEvent {
  tradeId: number;
  gameId: number;
  buyerUsername: string;
  sellerUsername: string;
  cellPosition: number;
  cellName: string;
}

export interface PropertyPurchasedEvent {
  gameId: number;
  cellId: number;
  ownerUsername: string;
  price: number;
}

export interface HouseBuiltEvent {
  gameId: number;
  cellId: number;
  cellName: string;
  ownerUsername: string;
  houses: number;
  price: number;
}

export interface HotelBuiltEvent {
  gameId: number;
  cellId: number;
  cellName: string;
  ownerUsername: string;
  hotels: number;
  price: number;
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
  public playerReady$ = new Subject<PlayerReadyEvent>();
  public gameStarted$ = new Subject<GameStartedEvent>();
  public playerJoined$ = new Subject<PlayerJoinedEvent>();
  public rentPaid$ = new Subject<RentPaidEvent>();
  public tradeProposed$ = new Subject<TradeProposedEvent>();
  public tradeAccepted$ = new Subject<TradeAcceptedEvent>();
  public tradeRejected$ = new Subject<TradeRejectedEvent>();
  public propertyPurchased$ = new Subject<PropertyPurchasedEvent>();
  public houseBuilt$ = new Subject<HouseBuiltEvent>();
  public hotelBuilt$ = new Subject<HotelBuiltEvent>();
  public playerPositions$ = new BehaviorSubject<Map<string, number>>(new Map());

  constructor(private authService: AuthService) {}

  public startConnection(): Promise<void> {
    // If already connected, return resolved promise
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return Promise.resolve();
    }

    // If connection exists but not connected, try to start it
    if (this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      console.log('SignalR connection in progress...');
      return Promise.resolve();
    }

    // Create new connection
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

    this.hubConnection.on('PlayerReady', (event: PlayerReadyEvent) => {
      this.playerReady$.next(event);
    });

    this.hubConnection.on('GameStarted', (event: GameStartedEvent) => {
      this.gameStarted$.next(event);
    });

    this.hubConnection.on('PlayerJoined', (event: PlayerJoinedEvent) => {
      console.log('SignalR received PlayerJoined event:', event);
      this.playerJoined$.next(event);
    });

    this.hubConnection.on('RentPaid', (event: RentPaidEvent) => {
      this.rentPaid$.next(event);
    });

    this.hubConnection.on('TradeProposed', (event: TradeProposedEvent) => {
      this.tradeProposed$.next(event);
    });

    this.hubConnection.on('TradeAccepted', (event: TradeAcceptedEvent) => {
      this.tradeAccepted$.next(event);
    });

    this.hubConnection.on('TradeRejected', (event: TradeRejectedEvent) => {
      this.tradeRejected$.next(event);
    });

    this.hubConnection.on('PropertyPurchased', (event: PropertyPurchasedEvent) => {
      this.propertyPurchased$.next(event);
    });

    this.hubConnection.on('HouseBuilt', (event: HouseBuiltEvent) => {
      this.houseBuilt$.next(event);
    });

    this.hubConnection.on('HotelBuilt', (event: HotelBuiltEvent) => {
      this.hotelBuilt$.next(event);
    });
  }
}
