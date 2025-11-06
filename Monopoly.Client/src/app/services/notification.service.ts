import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface GameInvitation {
  gameId: number;
  gameName: string;
  invitedBy: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private invitationsSubject = new BehaviorSubject<GameInvitation[]>([]);
  public invitations$: Observable<GameInvitation[]> = this.invitationsSubject.asObservable();

  constructor() {}

  addInvitation(invitation: GameInvitation): void {
    const current = this.invitationsSubject.value;
    // Check if invitation already exists
    const exists = current.some(inv => inv.gameId === invitation.gameId);
    if (!exists) {
      console.log('Adding invitation:', invitation);
      const updated = [...current, invitation];
      console.log('Updated invitations:', updated);
      this.invitationsSubject.next(updated);
    } else {
      console.log('Invitation already exists for game:', invitation.gameId);
    }
  }

  removeInvitation(gameId: number): void {
    const current = this.invitationsSubject.value;
    const filtered = current.filter(inv => inv.gameId !== gameId);
    this.invitationsSubject.next(filtered);
  }

  clearAllInvitations(): void {
    this.invitationsSubject.next([]);
  }

  getInvitations(): GameInvitation[] {
    return this.invitationsSubject.value;
  }
}
