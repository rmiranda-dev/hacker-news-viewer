import { TestBed } from '@angular/core/testing';
import { StoriesStateService } from './stories-state.service';
import { StoriesApiService } from './stories-api.service';
import { of, throwError } from 'rxjs';
import { PagedStories, Story } from '../models/story';
import { fakeAsync, tick } from '@angular/core/testing';

describe('StoriesStateService', () => {
  let service: StoriesStateService;
  let mockStoriesApiService: jasmine.SpyObj<StoriesApiService>;

  const mockStories: Story[] = [
    { id: 1, title: 'Test Story 1', url: 'https://example.com/1', by: 'user1', time: 1625097600 },
    { id: 2, title: 'Test Story 2', url: 'https://example.com/2', by: 'user2', time: 1625097700 }
  ];

  const mockPagedStories: PagedStories = {
    total: 100,
    items: mockStories
  };

  beforeEach(() => {
    const spy = jasmine.createSpyObj('StoriesApiService', ['getStories']);

    TestBed.configureTestingModule({
      providers: [
        { provide: StoriesApiService, useValue: spy }
      ]
    });

    service = TestBed.inject(StoriesStateService);
    mockStoriesApiService = TestBed.inject(StoriesApiService) as jasmine.SpyObj<StoriesApiService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default values', () => {
    service.pageIndex$.subscribe(value => expect(value).toBe(0));
    service.pageSize$.subscribe(value => expect(value).toBe(20));
    service.searchTerm$.subscribe(value => expect(value).toBe(''));
    service.loading$.subscribe(value => expect(value).toBe(false));
    service.error$.subscribe(value => expect(value).toBeNull());
    service.total$.subscribe(value => expect(value).toBe(0));
    service.stories$.subscribe(value => expect(value).toEqual([]));
  });

  it('should load stories on init', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.init();

    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(0, 20, undefined);
  });

  it('should update loading state during API call', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));
    
    const loadingStates: boolean[] = [];
    service.loading$.subscribe(loading => loadingStates.push(loading));

    service.init();
    tick();

    expect(loadingStates).toContain(true);
    expect(loadingStates[loadingStates.length - 1]).toBe(false);
  }));

  it('should update stories and total on successful API response', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.init();
    tick();

    service.stories$.subscribe(stories => expect(stories).toEqual(mockStories));
    service.total$.subscribe(total => expect(total).toBe(100));
  }));

  it('should debounce search input and only trigger API call once', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    // Simulate rapid search input
    service.setSearch('angular');
    service.setSearch('angulars');
    service.setSearch('angular');

    // Fast forward through debounce time
    tick(299);
    expect(mockStoriesApiService.getStories).not.toHaveBeenCalled();

    tick(1); // Complete the debounce
    expect(mockStoriesApiService.getStories).toHaveBeenCalledTimes(1);
    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(0, 20, 'angular');
  }));

  it('should reset page index to 0 when search term changes', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    // Set page index to 2
    service.setPageIndex(2);
    tick();
    
    let currentPageIndex = 2;
    service.pageIndex$.subscribe(pageIndex => currentPageIndex = pageIndex);
    
    expect(currentPageIndex).toBe(2);
    mockStoriesApiService.getStories.calls.reset();

    // Change search term
    service.setSearch('test');
    tick(300); // Wait for debounce

    expect(currentPageIndex).toBe(0);
    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(0, 20, 'test');
  }));

  it('should not trigger duplicate search for same term', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.setSearch('angular');
    tick(300);

    const callCount = mockStoriesApiService.getStories.calls.count();
    
    service.setSearch('angular'); // Same term
    tick(300);

    expect(mockStoriesApiService.getStories.calls.count()).toBe(callCount);
  }));

  it('should update page index and reload', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.setPageIndex(2);

    service.pageIndex$.subscribe(pageIndex => expect(pageIndex).toBe(2));
    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(2, 20, undefined);
  });

  it('should not reload if setting same page index', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.setPageIndex(0); // Same as initial value

    expect(mockStoriesApiService.getStories).not.toHaveBeenCalled();
  });

  it('should update page size, reset to page 0, and reload', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    // Set initial page index to 2
    service.setPageIndex(2);
    mockStoriesApiService.getStories.calls.reset();

    // Change page size
    service.setPageSize(10);

    service.pageSize$.subscribe(pageSize => expect(pageSize).toBe(10));
    service.pageIndex$.subscribe(pageIndex => expect(pageIndex).toBe(0));
    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(0, 10, undefined);
  });

  it('should not reload if setting same page size', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.setPageSize(20); // Same as initial value

    expect(mockStoriesApiService.getStories).not.toHaveBeenCalled();
  });

  it('should calculate correct offset for different page indices', () => {
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));

    service.setPageSize(15);
    mockStoriesApiService.getStories.calls.reset();

    service.setPageIndex(3);

    expect(mockStoriesApiService.getStories).toHaveBeenCalledWith(3, 15, undefined);
  });

  it('should handle API error and set error message', fakeAsync(() => {
    const errorMessage = 'Network error';
    mockStoriesApiService.getStories.and.returnValue(throwError({ message: errorMessage }));

    service.init();
    tick();

    service.error$.subscribe(error => expect(error).toBe(errorMessage));
    service.loading$.subscribe(loading => expect(loading).toBe(false));
  }));

  it('should handle API error without message and set default error', fakeAsync(() => {
    mockStoriesApiService.getStories.and.returnValue(throwError({}));

    service.init();
    tick();

    service.error$.subscribe(error => expect(error).toBe('An error occurred while loading stories'));
  }));

  it('should clear error on successful API call', fakeAsync(() => {
    // First, cause an error
    mockStoriesApiService.getStories.and.returnValue(throwError({ message: 'Error' }));
    service.init();
    tick();

    // Then, make a successful call
    mockStoriesApiService.getStories.and.returnValue(of(mockPagedStories));
    service.reload();
    tick();

    service.error$.subscribe(error => expect(error).toBeNull());
  }));
});
