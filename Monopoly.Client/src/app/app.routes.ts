import { Routes } from '@angular/router';
import { RegisterComponent } from './register/register.component';
import { LoginComponent } from './login/login.component';
import { CreateGameComponent } from './create-game/create-game';
import { PlaygroundComponent } from './playground/playground';
import { MyGamesComponent } from './my-games/my-games.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'register', component: RegisterComponent, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent, canActivate: [authGuard] },
  { path: 'create-game', component: CreateGameComponent },
  { path: 'my-games', component: MyGamesComponent },
  { path: 'playground/:id', component: PlaygroundComponent },
  { path: '', redirectTo: '', pathMatch: 'full' }
];
