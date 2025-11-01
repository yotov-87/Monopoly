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
}
