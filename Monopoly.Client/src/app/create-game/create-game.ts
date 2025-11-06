import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GameService } from '../services/game.service';
import { MonopolyBoardComponent } from '../components/monopoly-board/monopoly-board.component';

interface PropertyConfig {
  position: number;
  name: string;
  colorGroup: string;
  price: number;
  rent: number;
  housePrice: number;
  hotelPrice: number;
}

@Component({
  selector: 'app-create-game',
  imports: [FormsModule, CommonModule, MonopolyBoardComponent],
  templateUrl: './create-game.html',
  styleUrl: './create-game.scss'
})
export class CreateGameComponent implements OnInit {
  gameName = '';
  playerCount = 2;
  minPlayers = 2;
  maxPlayers = 8;
  currentStep = 1; // Step 1: Game settings, Step 2: Board configuration
  
  // Property configuration
  properties: PropertyConfig[] = [];

  constructor(private router: Router, private gameService: GameService) {}

  ngOnInit(): void {
    this.initializeProperties();
  }

  initializeProperties(): void {
    // Initialize all properties with default values
    const propertyData: Omit<PropertyConfig, 'price' | 'rent' | 'housePrice' | 'hotelPrice'>[] = [
      { position: 1, name: "Mediterranean Avenue", colorGroup: "Brown" },
      { position: 3, name: "Baltic Avenue", colorGroup: "Brown" },
      { position: 5, name: "Reading Railroad", colorGroup: "Railroad" },
      { position: 6, name: "Oriental Avenue", colorGroup: "LightBlue" },
      { position: 8, name: "Vermont Avenue", colorGroup: "LightBlue" },
      { position: 9, name: "Connecticut Avenue", colorGroup: "LightBlue" },
      { position: 11, name: "St. Charles Place", colorGroup: "Pink" },
      { position: 13, name: "States Avenue", colorGroup: "Pink" },
      { position: 14, name: "Virginia Avenue", colorGroup: "Pink" },
      { position: 15, name: "Pennsylvania Railroad", colorGroup: "Railroad" },
      { position: 16, name: "St. James Place", colorGroup: "Orange" },
      { position: 18, name: "Tennessee Avenue", colorGroup: "Orange" },
      { position: 19, name: "New York Avenue", colorGroup: "Orange" },
      { position: 21, name: "Kentucky Avenue", colorGroup: "Red" },
      { position: 23, name: "Indiana Avenue", colorGroup: "Red" },
      { position: 24, name: "Illinois Avenue", colorGroup: "Red" },
      { position: 25, name: "B. & O. Railroad", colorGroup: "Railroad" },
      { position: 26, name: "Atlantic Avenue", colorGroup: "Yellow" },
      { position: 27, name: "Ventnor Avenue", colorGroup: "Yellow" },
      { position: 12, name: "Electric Company", colorGroup: "Utility" },
      { position: 28, name: "Water Works", colorGroup: "Utility" },
      { position: 29, name: "Marvin Gardens", colorGroup: "Yellow" },
      { position: 31, name: "Pacific Avenue", colorGroup: "Green" },
      { position: 32, name: "North Carolina Avenue", colorGroup: "Green" },
      { position: 34, name: "Pennsylvania Avenue", colorGroup: "Green" },
      { position: 35, name: "Short Line Railroad", colorGroup: "Railroad" },
      { position: 37, name: "Park Place", colorGroup: "DarkBlue" },
      { position: 39, name: "Boardwalk", colorGroup: "DarkBlue" }
    ];

    this.properties = propertyData.map(prop => ({ 
      ...prop,
      price: 10,
      rent: 10,
      housePrice: 10,
      hotelPrice: 10
    }));
  }

  onNextStep(): void {
    if (!this.gameName.trim()) {
      alert('Please enter a game name');
      return;
    }

    if (this.playerCount < this.minPlayers || this.playerCount > this.maxPlayers) {
      alert(`Player count must be between ${this.minPlayers} and ${this.maxPlayers}`);
      return;
    }

    this.currentStep = 2;
  }

  onBackStep(): void {
    this.currentStep = 1;
  }

  onCreateGame(): void {
    const customProperties = this.properties.map(prop => ({
      position: prop.position,
      price: prop.price,
      rent: prop.rent,
      housePrice: prop.housePrice,
      hotelPrice: prop.hotelPrice
    }));

    this.gameService.createGame({
      name: this.gameName,
      playerCount: this.playerCount,
      customRents: customProperties
    }).subscribe({
      next: (response) => {
        console.log('Game created successfully', response);
        // Navigate to the playground with the game ID
        this.router.navigate(['/playground', response.id]);
      },
      error: (error) => {
        console.error('Failed to create game', error);
        alert('Failed to create game: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }

  getColorGroupColor(colorGroup: string): string {
    const colorMap: { [key: string]: string } = {
      'Brown': '#8B4513',
      'LightBlue': '#87CEEB',
      'Pink': '#FF1493',
      'Orange': '#FFA500',
      'Red': '#FF0000',
      'Yellow': '#FFFF00',
      'Green': '#00FF00',
      'DarkBlue': '#0000CD',
      'Railroad': '#000000',
      'Utility': '#888888'
    };
    return colorMap[colorGroup] || '#666';
  }

  groupPropertiesByColor(): { [key: string]: PropertyConfig[] } {
    const grouped: { [key: string]: PropertyConfig[] } = {};
    this.properties.forEach(prop => {
      if (!grouped[prop.colorGroup]) {
        grouped[prop.colorGroup] = [];
      }
      grouped[prop.colorGroup].push(prop);
    });
    return grouped;
  }

  getColorGroups(): string[] {
    return ['Brown', 'LightBlue', 'Pink', 'Orange', 'Red', 'Yellow', 'Green', 'DarkBlue', 'Railroad', 'Utility'];
  }
}
