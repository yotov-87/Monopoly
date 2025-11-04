import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoardCell } from '../../services/game.service';

@Component({
  selector: 'app-cell-info-popup',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cell-info-popup.component.html',
  styleUrls: ['./cell-info-popup.component.scss']
})
export class CellInfoPopupComponent {
  @Input() cell: BoardCell | null = null;
  @Output() close = new EventEmitter<void>();

  getCellTypeName(cellType: number): string {
    const types: { [key: number]: string } = {
      0: 'Start (GO)',
      1: 'Property',
      2: 'Tax',
      3: 'Chance',
      4: 'Community Chest',
      5: 'Jail',
      6: 'Free Parking',
      7: 'Go To Jail',
      8: 'Utility',
      9: 'Railroad'
    };
    return types[cellType] || 'Unknown';
  }

  getNeighborhoodColor(name: string): string {
    // Extract color from property name or use default
    if (name.includes('Mediterranean') || name.includes('Baltic')) return '#8B4513';
    if (name.includes('Oriental') || name.includes('Vermont') || name.includes('Connecticut')) return '#87CEEB';
    if (name.includes('St. Charles') || name.includes('States') || name.includes('Virginia')) return '#FF1493';
    if (name.includes('St. James') || name.includes('Tennessee') || name.includes('New York')) return '#FFA500';
    if (name.includes('Kentucky') || name.includes('Indiana') || name.includes('Illinois')) return '#FF0000';
    if (name.includes('Atlantic') || name.includes('Ventnor') || name.includes('Marvin')) return '#FFFF00';
    if (name.includes('Pacific') || name.includes('North Carolina') || name.includes('Pennsylvania')) return '#008000';
    if (name.includes('Park Place') || name.includes('Boardwalk')) return '#0000FF';
    return '#666';
  }

  onClose(): void {
    this.close.emit();
  }

  onOverlayClick(event: MouseEvent): void {
    // Close only if clicking on overlay, not on modal content
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
