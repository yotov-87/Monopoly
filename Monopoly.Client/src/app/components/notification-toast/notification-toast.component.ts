import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, DoCheck } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Notification {
  id: number;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  icon?: string;
}

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-toast.component.html',
  styleUrls: ['./notification-toast.component.scss']
})
export class NotificationToastComponent implements OnInit, OnDestroy, DoCheck {
  @Input() notifications: Notification[] = [];
  @Output() remove = new EventEmitter<number>();

  private timers: Map<number, any> = new Map();

  ngOnInit(): void {
    // Initial setup
  }

  ngOnDestroy(): void {
    // Clear all timers
    this.timers.forEach(timer => clearTimeout(timer));
    this.timers.clear();
  }

  ngDoCheck(): void {
    // Check for new notifications and start timers
    this.notifications.forEach(notification => {
      if (!this.timers.has(notification.id)) {
        this.startAutoRemove(notification.id);
      }
    });
  }

  startAutoRemove(id: number): void {
    const timer = setTimeout(() => {
      this.removeNotification(id);
    }, 10000); // 10 seconds
    this.timers.set(id, timer);
  }

  removeNotification(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.remove.emit(id);
  }
}
