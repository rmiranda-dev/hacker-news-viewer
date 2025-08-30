import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate } from '@angular/animations';
import { Observable } from 'rxjs';
import { Story } from '../../models/story';
import { StoriesStateService } from '../../services/stories-state.service';
import { SearchBoxComponent } from './search-box.component';
import { StoryListComponent } from './story-list.component';

@Component({
  selector: 'app-stories-page',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    SearchBoxComponent,
    StoryListComponent
  ],
  template: `
    <div class="stories-page">
      <!-- Header -->
      <mat-toolbar color="primary" class="header-toolbar">
        <mat-icon class="header-icon">article</mat-icon>
        <h1 class="header-title">Hacker News Viewer</h1>
      </mat-toolbar>

      <div class="content-container">
        <!-- Search Box -->
        <app-search-box (searchChange)="onSearch($event)"></app-search-box>

        <!-- Loading Spinner -->
        <div class="loading-container" *ngIf="loading$ | async">
          <mat-progress-spinner 
            mode="indeterminate" 
            diameter="50">
          </mat-progress-spinner>
          <p class="loading-text">Loading stories...</p>
        </div>

        <!-- Error State -->
        <mat-card class="error-card" *ngIf="error$ | async as errorMessage">
          <mat-card-content>
            <div class="error-content">
              <mat-icon class="error-icon" color="warn">error</mat-icon>
              <h3>Something went wrong</h3>
              <p>{{ errorMessage }}</p>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Story List -->
        <app-story-list 
          *ngIf="(loading$ | async) === false && (error$ | async) === null"
          [stories]="stories$ | async">
        </app-story-list>

        <!-- Pagination -->
        <div class="pagination-container" *ngIf="(total$ | async) && (total$ | async)! > 0">
          <mat-paginator
            [length]="total$ | async"
            [pageSize]="pageSize$ | async"
            [pageIndex]="pageIndex$ | async"
            [pageSizeOptions]="[10, 20, 50, 100]"
            (page)="onPageChange($event)"
            showFirstLastButtons>
          </mat-paginator>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .stories-page {
      min-height: 100vh;
      background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
      position: relative;
    }

    .stories-page::before {
      content: '';
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: radial-gradient(circle at 20% 50%, rgba(63, 81, 181, 0.1) 0%, transparent 50%),
                  radial-gradient(circle at 80% 20%, rgba(33, 150, 243, 0.1) 0%, transparent 50%),
                  radial-gradient(circle at 40% 80%, rgba(0, 188, 212, 0.1) 0%, transparent 50%);
      pointer-events: none;
      z-index: 0;
    }

    .header-toolbar {
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: 0 4px 20px rgba(63, 81, 181, 0.15);
      background: linear-gradient(135deg, #3f51b5 0%, #5c6bc0 100%);
      backdrop-filter: blur(10px);
      border-bottom: 1px solid rgba(255, 255, 255, 0.2);
    }

    .header-icon {
      margin-right: 12px;
      font-size: 28px;
      animation: pulse 2s ease-in-out infinite alternate;
    }

    @keyframes pulse {
      0% { opacity: 0.8; }
      100% { opacity: 1; }
    }

    .header-title {
      margin: 0;
      font-size: 1.6rem;
      font-weight: 600;
      letter-spacing: 0.5px;
      background: linear-gradient(45deg, #ffffff, #e8eaf6);
      background-clip: text;
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      text-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }

    .content-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 32px 16px;
      position: relative;
      z-index: 1;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 64px 16px;
      min-height: 400px;
    }

    .loading-container mat-spinner {
      margin-bottom: 24px;
    }

    .loading-text {
      margin-top: 16px;
      color: #666;
      font-size: 1rem;
      font-weight: 500;
      animation: fadeInOut 2s ease-in-out infinite;
    }

    @keyframes fadeInOut {
      0%, 100% { opacity: 0.5; }
      50% { opacity: 1; }
    }

    .error-card {
      margin: 32px auto;
      max-width: 600px;
      background: linear-gradient(135deg, #ffebee 0%, #fce4ec 100%);
      border: none;
      border-radius: 16px;
      box-shadow: 0 8px 32px rgba(244, 67, 54, 0.15);
      overflow: hidden;
      position: relative;
    }

    .error-card::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 4px;
      background: linear-gradient(90deg, #f44336, #e91e63);
    }

    .error-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 32px 24px;
    }

    .error-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 20px;
      color: #f44336;
      animation: shake 0.5s ease-in-out;
    }

    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      25% { transform: translateX(-5px); }
      75% { transform: translateX(5px); }
    }

    .error-content h3 {
      margin: 8px 0 16px 0;
      color: #d32f2f;
      font-weight: 600;
      font-size: 1.5rem;
    }

    .error-content p {
      margin: 0;
      color: #666;
      font-size: 1rem;
      line-height: 1.6;
    }

    .pagination-container {
      display: flex;
      justify-content: center;
      margin-top: 48px;
      padding: 24px;
    }

    .pagination-container ::ng-deep .mat-mdc-paginator {
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 16px rgba(0,0,0,0.1);
      border: 1px solid #e0e0e0;
    }

    .pagination-container ::ng-deep .mat-mdc-paginator .mat-mdc-icon-button {
      transition: all 0.2s ease;
    }

    .pagination-container ::ng-deep .mat-mdc-paginator .mat-mdc-icon-button:hover {
      background-color: rgba(63, 81, 181, 0.1);
      transform: scale(1.05);
    }

    @media (max-width: 600px) {
      .content-container {
        padding: 24px 12px;
      }
      
      .header-title {
        font-size: 1.3rem;
      }

      .header-icon {
        font-size: 24px;
        margin-right: 8px;
      }

      .loading-container {
        padding: 48px 16px;
        min-height: 300px;
      }

      .error-content {
        padding: 24px 16px;
      }

      .error-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
      }

      .error-content h3 {
        font-size: 1.25rem;
      }

      .pagination-container {
        margin-top: 32px;
        padding: 16px;
      }
    }
  `],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.5s cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class StoriesPageComponent implements OnInit {
  // Inject the service using modern Angular patterns
  private readonly storiesStateService = inject(StoriesStateService);

  // Observables from the state service
  stories$: Observable<Story[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  total$: Observable<number>;
  pageSize$: Observable<number>;
  pageIndex$: Observable<number>;

  constructor() {
    // Initialize observables
    this.stories$ = this.storiesStateService.stories$;
    this.loading$ = this.storiesStateService.loading$;
    this.error$ = this.storiesStateService.error$;
    this.total$ = this.storiesStateService.total$;
    this.pageSize$ = this.storiesStateService.pageSize$;
    this.pageIndex$ = this.storiesStateService.pageIndex$;
  }

  ngOnInit(): void {
    // Initialize and load initial stories
    this.storiesStateService.init();
  }

  onSearch(searchTerm: string): void {
    this.storiesStateService.setSearch(searchTerm);
  }

  private currentPageSize = 20; // Track current page size

  onPageChange(event: PageEvent): void {
    // Check if page size changed
    if (event.pageSize !== this.currentPageSize) {
      this.currentPageSize = event.pageSize;
      this.storiesStateService.setPageSize(event.pageSize);
    } else {
      // Only update page index if page size didn't change
      this.storiesStateService.setPageIndex(event.pageIndex);
    }
  }
}
