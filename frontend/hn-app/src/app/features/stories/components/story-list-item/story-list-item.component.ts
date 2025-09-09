import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Story } from '../../../../models/story';

@Component({
  selector: 'app-story-list-item',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './story-list-item.component.html',
  styleUrls: ['./story-list-item.component.css']
})
export class StoryListItemComponent {
  @Input() story!: Story;
}
