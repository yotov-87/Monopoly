import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BoardCellComponent } from '../board-cell/board-cell.component';
import { BoardCell } from '../../services/game.service';

@Component({
  selector: 'app-monopoly-board',
  standalone: true,
  imports: [CommonModule, BoardCellComponent],
  templateUrl: './monopoly-board.component.html',
  styleUrls: ['./monopoly-board.component.scss']
})
export class MonopolyBoardComponent {
  @Input() boardCells: BoardCell[] = [];
  @Input() currentPlayerColor: string = '#2e7d32';
  @Input() currentTurnUsername: string = '';
  @Input() isGameInactive: boolean = false;

  // Board configuration
  private readonly STANDARD_BOARD_SIZE = 40;
  private readonly CELLS_PER_SIDE = 11; // Including corners

  /**
   * Get cells for bottom row (positions 0, 39-31, 30)
   * Order: left to right - GO(0), 39-31, GO TO JAIL(30)
   */
  getBottomCells(): number[] {
    return [0, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30];
  }

  /**
   * Get cells for right column (positions 21-29)
   * Order: top to bottom
   */
  getRightCells(): number[] {
    return [21, 22, 23, 24, 25, 26, 27, 28, 29];
  }

  /**
   * Get cells for top row (positions 10-20)
   * Order: left to right - JAIL(10), 11-19, FREE PARKING(20)
   */
  getTopCells(): number[] {
    return [10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20];
  }

  /**
   * Get cells for left column (positions 1-9)
   * Order: top to bottom (9 to 1)
   */
  getLeftCells(): number[] {
    return [9, 8, 7, 6, 5, 4, 3, 2, 1];
  }

  /**
   * Get board cell by position
   */
  getBoardCell(position: number): BoardCell | undefined {
    return this.boardCells.find(cell => cell.position === position);
  }

  /**
   * Check if cell has current player
   */
  cellHasCurrentPlayer(position: number): boolean {
    const cell = this.getBoardCell(position);
    return cell?.playersHere?.some(p => p.username === this.currentTurnUsername) || false;
  }

  /**
   * Get player colors at cell
   */
  getCellPlayerColors(position: number): string[] {
    const cell = this.getBoardCell(position);
    return cell?.playersHere?.map(p => p.color) || [];
  }

  /**
   * Get border style for cell
   */
  getCellBorderStyle(position: number): any {
    if (this.cellHasCurrentPlayer(position)) {
      return {
        'border-color': this.currentPlayerColor,
        'border-width': '4px'
      };
    }
    return {};
  }

  /**
   * Get owner color for cell
   */
  getOwnerColor(position: number): string | null {
    const cell = this.getBoardCell(position);
    if (!cell?.ownerUsername) return null;
    
    // Find the owner player across all cells
    const owner = this.boardCells
      .flatMap(c => c.playersHere)
      .find(p => p.username === cell.ownerUsername);
    
    return owner?.color || null;
  }

  /**
   * Check if position is corner
   */
  isCorner(position: number): boolean {
    return position === 0 || position === 10 || position === 20 || position === 30;
  }

  /**
   * Track by function for performance
   */
  trackByPosition(index: number, pos: number): number {
    return pos;
  }
}
