import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BoardCell } from '../../services/game.service';

@Component({
  selector: 'app-trade-offer-popup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trade-offer-popup.component.html',
  styleUrls: ['./trade-offer-popup.component.scss']
})
export class TradeOfferPopupComponent {
  @Input() cell: BoardCell | null = null;
  @Input() playerMoney: number = 0;
  @Output() makeOffer = new EventEmitter<number>();
  @Output() close = new EventEmitter<void>();

  offeredPrice: number = 0;

  onClose(): void {
    this.close.emit();
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }

  onMakeOffer(): void {
    if (this.offeredPrice > 0 && this.offeredPrice <= this.playerMoney) {
      this.makeOffer.emit(this.offeredPrice);
    }
  }

  canAfford(): boolean {
    return this.offeredPrice > 0 && this.offeredPrice <= this.playerMoney;
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
