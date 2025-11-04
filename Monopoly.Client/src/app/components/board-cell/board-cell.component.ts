import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoardCell } from '../../services/game.service';

@Component({
  selector: 'app-board-cell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board-cell.component.html',
  styleUrls: ['./board-cell.component.scss']
})
export class BoardCellComponent {
  @Input() cell!: BoardCell;
  @Input() isCorner: boolean = false;
  @Input() isCurrentPlayer: boolean = false;
  @Input() hasPlayers: boolean = false;
  @Input() borderStyle: any = {};
  @Input() position!: number;
  @Input() rowType: 'bottom' | 'top' | 'left' | 'right' = 'bottom';
  @Input() currentPlayerColor: string = '#2e7d32';
  @Input() currentTurnUsername: string = '';

  isPlayerCurrentTurn(playerUsername: string): boolean {
    return this.currentTurnUsername === playerUsername;
  }

  getColorGroupBorderColor(): string | null {
    if (!this.cell?.colorGroup) return null;
    
    const colorMap: { [key: string]: string } = {
      'Brown': '#8B4513',
      'LightBlue': '#87CEEB',
      'Pink': '#FF1493',
      'Orange': '#FFA500',
      'Red': '#FF0000',
      'Yellow': '#FFFF00',
      'Green': '#00FF00',
      'DarkBlue': '#0000CD'
    };
    
    return colorMap[this.cell.colorGroup] || null;
  }
}
