import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate } from '@angular/animations';
import { Observable } from 'rxjs';
import { Story } from '../../../../models/story';
import { StoriesStateService } from '../../../../services/stories-state.service';
import { SearchBoxComponent, StoryListComponent } from '../../components';

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
  templateUrl: './stories-page.component.html',
  styleUrls: ['./stories-page.component.css'],
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
