import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate } from '@angular/animations';
import { Story } from '../../models/story';
import { StoryListItemComponent } from './story-list-item.component';

@Component({
  selector: 'app-story-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    StoryListItemComponent
  ],
  template: `
    <div class="story-list-container">
      <ng-container *ngIf="stories && stories.length > 0; else emptyState">
        <div class="stories-grid">
          <app-story-list-item 
            *ngFor="let story of stories; trackBy: trackByStoryId; let i = index"
            [story]="story"
            class="story-item"
            [style.animation-delay]="(i * 150) + 'ms'">
          </app-story-list-item>
        </div>
      </ng-container>
      
      <ng-template #emptyState>
        <mat-card [@fadeIn] class="empty-state">
          <mat-card-content>
            <div class="empty-content">
              <mat-icon class="empty-icon">article</mat-icon>
              <h3>No stories found</h3>
              <p>Try adjusting your search terms or browse different pages.</p>
            </div>
          </mat-card-content>
        </mat-card>
      </ng-template>
    </div>
  `,
  styles: [`
    .story-list-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 16px;
      min-height: 200px;
    }
    
    .stories-grid {
      display: flex;
      flex-direction: column;
      gap: 0;
    }
    
    .story-item {
      opacity: 0;
      transform: translateY(30px);
      animation: slideInUp 0.8s cubic-bezier(0.4, 0, 0.2, 1) forwards;
    }
    
    @keyframes slideInUp {
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    .empty-state {
      margin: 32px 0;
      text-align: center;
      border-radius: 16px;
      background: linear-gradient(135deg, #f5f5f5 0%, #e8eaf6 100%);
      box-shadow: 0 8px 32px rgba(0,0,0,0.08);
      border: none;
    }
    
    .empty-content {
      padding: 48px 24px;
    }
    
    .empty-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #3f51b5;
      margin-bottom: 20px;
      opacity: 0.7;
      animation: float 3s ease-in-out infinite;
    }
    
    @keyframes float {
      0%, 100% { transform: translateY(0px); }
      50% { transform: translateY(-8px); }
    }
    
    .empty-content h3 {
      margin: 16px 0 8px 0;
      color: #424242;
      font-weight: 500;
      font-size: 1.5rem;
    }
    
    .empty-content p {
      margin: 0;
      color: #666;
      font-size: 1rem;
      line-height: 1.5;
    }
    
    @media (max-width: 600px) {
      .story-list-container {
        padding: 12px;
      }
      
      .empty-content {
        padding: 32px 16px;
      }
      
      .empty-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
      }
      
      .empty-content h3 {
        font-size: 1.25rem;
      }
    }
  `],
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
