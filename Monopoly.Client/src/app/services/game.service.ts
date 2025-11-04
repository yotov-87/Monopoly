import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateGameRequest {
  name: string;
  playerCount: number;
}

export interface GameResponse {
  id: number;
  name: string;
  playerCount: number;
  createdBy: string;
  createdAt: string;
  status: string;
  isActive: boolean;
  currentPlayerCount: number;
  players: string[];
  currentTurnUsername?: string;
}

export interface AddPlayerRequest {
  username: string;
}

export interface BoardCell {
  id: number;
  position: number;
  cellType: number;
  cellTypeName: string;
  name: string;
  colorGroup?: string;
  price?: number;
  ownerUsername?: string;
  playersHere: PlayerPosition[];
}

export interface PlayerPosition {
  username: string;
  position: number;
  money: number;
  color: string;
  isReady: boolean;
}

export interface PlaygroundInfo {
  game: GameResponse;
  boardCells: BoardCell[];
  isCreator: boolean;
  hasAccess: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private apiUrl = 'http://localhost:5262/api/game';

  constructor(private http: HttpClient) {}

  createGame(request: CreateGameRequest): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}/create`, request);
  }

  getGame(id: number): Observable<GameResponse> {
    return this.http.get<GameResponse>(`${this.apiUrl}/${id}`);
  }

  getAllGames(): Observable<GameResponse[]> {
    return this.http.get<GameResponse[]>(`${this.apiUrl}/all`);
  }

  getMyGames(): Observable<GameResponse[]> {
    return this.http.get<GameResponse[]>(`${this.apiUrl}/my-games`);
  }

  addPlayer(gameId: number, username: string): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}/${gameId}/add-player`, { username });
  }

  getPlaygroundInfo(gameId: number): Observable<PlaygroundInfo> {
    return this.http.get<PlaygroundInfo>(`${this.apiUrl}/playground-info/${gameId}`);
  }

  endTurn(gameId: number): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}/${gameId}/end-turn`, {});
  }

  movePlayer(gameId: number, steps: number): Observable<PlaygroundInfo> {
    return this.http.post<PlaygroundInfo>(`${this.apiUrl}/${gameId}/move`, { steps });
  }

  rollDice(gameId: number, dice1: number, dice2: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${gameId}/roll-dice`, { dice1, dice2 });
  }

  setPlayerReady(gameId: number, isReady: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/${gameId}/ready`, { isReady });
  }

  purchaseProperty(gameId: number, cellId: number): Observable<PlaygroundInfo> {
    return this.http.post<PlaygroundInfo>(`${this.apiUrl}/${gameId}/purchase-property/${cellId}`, {});
  }

  payRent(gameId: number, cellId: number): Observable<PlaygroundInfo> {
    return this.http.post<PlaygroundInfo>(`${this.apiUrl}/${gameId}/pay-rent/${cellId}`, {});
  }

  proposeTrade(gameId: number, cellId: number, offeredPrice: number): Observable<{ tradeId: number, message: string }> {
    return this.http.post<{ tradeId: number, message: string }>(
      `${this.apiUrl}/${gameId}/propose-trade/${cellId}`, 
      { offeredPrice }
    );
  }

  respondToTrade(gameId: number, tradeId: number, accept: boolean): Observable<PlaygroundInfo> {
    return this.http.post<PlaygroundInfo>(
      `${this.apiUrl}/${gameId}/respond-trade/${tradeId}`, 
      { accept }
    );
  }
}
