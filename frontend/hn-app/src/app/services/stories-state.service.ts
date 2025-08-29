import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Subject, EMPTY } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError, finalize, skip } from 'rxjs/operators';
import { StoriesApiService } from './stories-api.service';
import { Story } from '../models/story';

@Injectable({
  providedIn: 'root'
})
export class StoriesStateService {
  private readonly storiesApiService = inject(StoriesApiService);

  // Private BehaviorSubjects for internal state management
  private readonly pageIndexSubject = new BehaviorSubject<number>(0);
  private readonly pageSizeSubject = new BehaviorSubject<number>(20);
  private readonly searchTermSubject = new BehaviorSubject<string>('');
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  private readonly errorSubject = new BehaviorSubject<string | null>(null);
  private readonly totalSubject = new BehaviorSubject<number>(0);
  private readonly storiesSubject = new BehaviorSubject<Story[]>([]);

  // Public observables for external consumption
  public readonly pageIndex$ = this.pageIndexSubject.asObservable();
  public readonly pageSize$ = this.pageSizeSubject.asObservable();
  public readonly searchTerm$ = this.searchTermSubject.asObservable();
  public readonly loading$ = this.loadingSubject.asObservable();
  public readonly error$ = this.errorSubject.asObservable();
  public readonly total$ = this.totalSubject.asObservable();
  public readonly stories$ = this.storiesSubject.asObservable();

  // Subject for triggering reload operations
  private readonly reloadSubject = new Subject<void>();

  constructor() {
    this.setupSearchDebounce();
    this.setupReloadHandler();
  }

  /**
   * Initialize the service and load the first page
   */
  init(): void {
    this.reload();
  }

  /**
   * Set search term with debouncing and reset to first page
   */
  setSearch(term: string): void {
    this.searchTermSubject.next(term);
  }

  /**
   * Set current page index and reload
   */
  setPageIndex(index: number): void {
    if (index !== this.pageIndexSubject.value) {
      this.pageIndexSubject.next(index);
      this.reload();
    }
  }

  /**
   * Set page size, reset to first page, and reload
   */
  setPageSize(size: number): void {
    if (size !== this.pageSizeSubject.value) {
      this.pageSizeSubject.next(size);
      this.pageIndexSubject.next(0); // Reset to first page
      this.reload();
    }
  }

  /**
   * Trigger a reload of stories with current state
   */
  reload(): void {
    this.reloadSubject.next();
  }

  /**
   * Setup debounced search handling
   */
  private setupSearchDebounce(): void {
    this.searchTermSubject.pipe(
      skip(1), // Skip the initial empty value
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      // Reset to first page when search changes
      this.pageIndexSubject.next(0);
      this.reloadSubject.next();
    });
  }

  /**
   * Setup reload handler that calls the API
   */
  private setupReloadHandler(): void {
    this.reloadSubject.pipe(
      switchMap(() => {
        this.loadingSubject.next(true);
        this.errorSubject.next(null);

        const pageIndex = this.pageIndexSubject.value;
        const pageSize = this.pageSizeSubject.value;
        const searchTerm = this.searchTermSubject.value;

        return this.storiesApiService.getStories(
          pageIndex,
          pageSize,
          searchTerm || undefined
        ).pipe(
          catchError((error) => {
            this.errorSubject.next(error.message || 'An error occurred while loading stories');
            return EMPTY;
          }),
          finalize(() => {
            this.loadingSubject.next(false);
          })
        );
      })
    ).subscribe((response) => {
      if (response) {
        this.storiesSubject.next(response.items);
        this.totalSubject.next(response.total);
      }
    });
  }
}
