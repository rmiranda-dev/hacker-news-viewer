import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StoriesPageComponent } from './features/stories';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, StoriesPageComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Hacker News Viewer';
}
