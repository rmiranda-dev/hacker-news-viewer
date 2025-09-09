import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate } from '@angular/animations';
import { Story } from '../../../../models/story';
import { StoryListItemComponent } from '../story-list-item/story-list-item.component';

@Component({
  selector: 'app-story-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    StoryListItemComponent
  ],
  templateUrl: './story-list.component.html',
  styleUrls: ['./story-list.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9) translateY(20px)' }),
        animate('0.6s cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ opacity: 1, transform: 'scale(1) translateY(0)' }))
      ])
    ])
  ]
})
export class StoryListComponent {
  @Input() stories: Story[] | null = [];
  
  trackByStoryId(index: number, story: Story): number {
    return story.id;
  }
}
