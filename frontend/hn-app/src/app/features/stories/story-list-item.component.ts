import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Story } from '../../models/story';

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
  template: `
    <mat-card class="story-card">
      <mat-card-content>
        <div class="story-header">
          <div class="story-title">
            <a 
              *ngIf="story.url; else noLinkTemplate"
              mat-button
              [href]="story.url"
              target="_blank"
              rel="noopener noreferrer"
              class="story-link">
              <mat-icon class="external-link-icon">open_in_new</mat-icon>
              {{ story.title || 'Untitled' }}
            </a>
            <ng-template #noLinkTemplate>
              <span class="no-link">
                <mat-icon class="article-icon">article</mat-icon>
                {{ story.title || 'Untitled' }}
              </span>
            </ng-template>
          </div>
        </div>
        
        <div class="story-meta">
          <div class="meta-item" *ngIf="story.by">
            <mat-icon class="meta-icon">person</mat-icon>
            <span class="author">{{ story.by }}</span>
          </div>
          <div class="meta-separator" *ngIf="story.by && story.time">•</div>
          <div class="meta-item" *ngIf="story.time">
            <mat-icon class="meta-icon">schedule</mat-icon>
            <span class="time">{{ story.time * 1000 | date:'medium' }}</span>
          </div>
          <div class="story-id">
            <mat-icon class="meta-icon">tag</mat-icon>
            <span>#{{ story.id }}</span>
          </div>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .story-card {
      margin-bottom: 16px;
      border-radius: 12px;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      border-left: 4px solid transparent;
      background: linear-gradient(145deg, #ffffff 0%, #fafafa 100%);
      position: relative;
      overflow: hidden;
    }
    
    .story-card::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 2px;
      background: linear-gradient(90deg, #3f51b5, #2196f3, #00bcd4);
      transform: scaleX(0);
      transition: transform 0.3s ease;
    }
    
    .story-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0,0,0,0.15);
      border-left-color: #3f51b5;
    }
    
    .story-card:hover::before {
      transform: scaleX(1);
    }
    
    .story-header {
      margin-bottom: 12px;
    }
    
    .story-title {
      display: flex;
      align-items: flex-start;
      gap: 8px;
    }
    
    .story-link {
      text-align: left;
      padding: 8px 12px;
      margin: -8px -12px;
      min-height: auto;
      line-height: 1.5;
      color: #1a237e;
      text-decoration: none;
      font-weight: 500;
      font-size: 1.1rem;
      border-radius: 8px;
      transition: all 0.2s ease;
      display: flex;
      align-items: flex-start;
      gap: 8px;
      width: 100%;
      justify-content: flex-start;
    }
    
    .story-link:hover {
      background-color: rgba(63, 81, 181, 0.04);
      color: #3f51b5;
      transform: translateX(4px);
    }
    
    .external-link-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      margin-top: 2px;
      opacity: 0.7;
      transition: all 0.2s ease;
    }
    
    .story-link:hover .external-link-icon {
      opacity: 1;
      transform: scale(1.1);
    }
    
    .no-link {
      font-weight: 500;
      color: #424242;
      font-size: 1.1rem;
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
    }
    
    .article-icon {
      color: #666;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
    
    .story-meta {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
      margin-top: 12px;
      padding-top: 12px;
      border-top: 1px solid #e0e0e0;
    }
    
    .meta-item {
      display: flex;
      align-items: center;
      gap: 4px;
      color: #666;
      font-size: 0.875rem;
      padding: 4px 8px;
      border-radius: 16px;
      background-color: rgba(0,0,0,0.04);
      transition: all 0.2s ease;
    }
    
    .meta-item:hover {
      background-color: rgba(63, 81, 181, 0.08);
      color: #3f51b5;
    }
    
    .meta-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      opacity: 0.7;
    }
    
    .meta-separator {
      color: #bbb;
      font-weight: bold;
      margin: 0 4px;
    }
    
    .author {
      font-weight: 600;
      color: #3f51b5;
    }
    
    .time {
      font-style: italic;
      color: #666;
    }
    
    .story-id {
      margin-left: auto;
      display: flex;
      align-items: center;
      gap: 4px;
      color: #999;
      font-size: 0.75rem;
      font-family: 'Courier New', monospace;
    }
    
    @media (max-width: 600px) {
      .story-meta {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
      
      .story-id {
        margin-left: 0;
      }
      
      .meta-separator {
        display: none;
      }
    }
  `],
  animations: [
    // Removed individual item animations since staggering is handled by parent
  ]
})
export class StoryListItemComponent {
  @Input() story!: Story;
}
