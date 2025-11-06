import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GameTemplate {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
}

export interface GameTemplateDetail {
  id: number;
  name: string;
  description?: string;
  propertyConfigurations: PropertyConfig[];
  createdAt: string;
}

export interface PropertyConfig {
  position: number;
  name: string;
  colorGroup: string;
  price: number;
  rent: number;
  housePrice: number;
  hotelPrice: number;
}

export interface SaveTemplateRequest {
  name: string;
  description?: string;
  propertyConfigurations: PropertyConfig[];
}

@Injectable({
  providedIn: 'root'
})
export class GameTemplateService {
  private apiUrl = 'http://localhost:5262/api/gametemplate';

  constructor(private http: HttpClient) {}

  saveTemplate(request: SaveTemplateRequest): Observable<GameTemplate> {
    return this.http.post<GameTemplate>(this.apiUrl, request);
  }

  getUserTemplates(): Observable<GameTemplate[]> {
    return this.http.get<GameTemplate[]>(this.apiUrl);
  }

  getTemplateById(id: number): Observable<GameTemplateDetail> {
    return this.http.get<GameTemplateDetail>(`${this.apiUrl}/${id}`);
  }

  deleteTemplate(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
