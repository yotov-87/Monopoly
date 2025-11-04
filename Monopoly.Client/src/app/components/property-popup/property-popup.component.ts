import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoardCell } from '../../services/game.service';

@Component({
  selector: 'app-property-popup',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './property-popup.component.html',
  styleUrls: ['./property-popup.component.scss']
})
export class PropertyPopupComponent {
  @Input() cell: BoardCell | null = null;
  @Input() playerMoney: number = 0;
  @Output() purchase = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();

  onPurchase(): void {
    this.purchase.emit();
  }

  onClose(): void {
    this.close.emit();
  }

  canAfford(): boolean {
    return this.cell?.price ? this.playerMoney >= this.cell.price : false;
  }
}
