import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { WeatherService } from './services/weather.service';
import { WeatherForecast } from './models/weather-forecast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = 'Monopoly.Client';
  weatherForecasts: WeatherForecast[] = [];
  showWeather = false;

  constructor(private weatherService: WeatherService) {}

  toggleWeather(): void {
    this.showWeather = !this.showWeather;
    if (this.showWeather && this.weatherForecasts.length === 0) {
      this.weatherService.getWeatherForecast()
        .subscribe(forecasts => this.weatherForecasts = forecasts,
                   err => { console.error('Failed to load weather', err); });
    }
  }
}