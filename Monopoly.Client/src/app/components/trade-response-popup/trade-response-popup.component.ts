import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoardCell } from '../../services/game.service';

export interface TradeOffer {
  tradeId: number;
  buyerUsername: string;
  cellPosition: number;
  cellName: string;
  offeredPrice: number;
}

@Component({
  selector: 'app-trade-response-popup',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trade-response-popup.component.html',
  styleUrls: ['./trade-response-popup.component.scss']
})
export class TradeResponsePopupComponent {
  @Input() tradeOffer: TradeOffer | null = null;
  @Output() accept = new EventEmitter<void>();
  @Output() reject = new EventEmitter<void>();

  onAccept(): void {
    this.accept.emit();
  }

  onReject(): void {
    this.reject.emit();
  }

  getNeighborhoodColor(name: string): string {
    const neighborhoodColors: { [key: string]: string } = {
      'Mediterranean': '#8B4513',
      'Baltic': '#8B4513',
      'Oriental': '#87CEEB',
      'Vermont': '#87CEEB',
      'Connecticut': '#87CEEB',
      'St. Charles': '#FF1493',
      'States': '#FF1493',
      'Virginia': '#FF1493',
      'St. James': '#FFA500',
      'Tennessee': '#FFA500',
      'New York': '#FFA500',
      'Kentucky': '#FF0000',
      'Indiana': '#FF0000',
      'Illinois': '#FF0000',
      'Atlantic': '#FFFF00',
      'Ventnor': '#FFFF00',
      'Marvin': '#FFFF00',
      'Pacific': '#00FF00',
      'North Carolina': '#00FF00',
      'Pennsylvania': '#00FF00',
      'Park': '#0000FF',
      'Boardwalk': '#0000FF'
    };

    for (const [neighborhood, color] of Object.entries(neighborhoodColors)) {
      if (name.includes(neighborhood)) {
        return color;
      }
    }
    return '#666';
  }
}
