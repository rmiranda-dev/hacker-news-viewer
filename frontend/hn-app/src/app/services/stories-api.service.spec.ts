import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { StoriesApiService } from './stories-api.service';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { PagedStories } from '../models/story';

describe('StoriesApiService', () => {
  let service: StoriesApiService;
  let httpMock: HttpTestingController;
  const mockApiBaseUrl = 'http://localhost:5000';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: API_BASE_URL, useValue: mockApiBaseUrl }
      ]
    });
    service = TestBed.inject(StoriesApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get stories with correct URL', () => {
    const pageIndex = 0;
    const pageSize = 10;
    const mockResponse: PagedStories = {
      total: 100,
      items: [
        {
          id: 1,
          title: 'Test Story',
          url: 'https://example.com',
          by: 'testuser',
          time: 1625097600
        }
      ]
    };

    service.getStories(pageIndex, pageSize).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const expectedUrl = `${mockApiBaseUrl}/api/stories/new?offset=0&limit=10`;
    const req = httpMock.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get stories with search query parameter', () => {
    const pageIndex = 1;
    const pageSize = 20;
    const search = 'angular';
    const mockResponse: PagedStories = {
      total: 50,
      items: []
    };

    service.getStories(pageIndex, pageSize, search).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const expectedUrl = `${mockApiBaseUrl}/api/stories/new?offset=20&limit=20&search=angular`;
    const req = httpMock.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should handle search parameter with special characters', () => {
    const pageIndex = 0;
    const pageSize = 10;
    const search = 'hello world & special chars!';
    const mockResponse: PagedStories = {
      total: 0,
      items: []
    };

    service.getStories(pageIndex, pageSize, search).subscribe();

    const expectedUrl = `${mockApiBaseUrl}/api/stories/new?offset=0&limit=10&search=${encodeURIComponent(search)}`;
    const req = httpMock.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should calculate offset correctly for different page indices', () => {
    const pageSize = 15;
    const mockResponse: PagedStories = { total: 0, items: [] };

    // Test page index 2
    service.getStories(2, pageSize).subscribe();
    const req1 = httpMock.expectOne(`${mockApiBaseUrl}/api/stories/new?offset=30&limit=15`);
    expect(req1.request.method).toBe('GET');
    req1.flush(mockResponse);

    // Test page index 5
    service.getStories(5, pageSize).subscribe();
    const req2 = httpMock.expectOne(`${mockApiBaseUrl}/api/stories/new?offset=75&limit=15`);
    expect(req2.request.method).toBe('GET');
    req2.flush(mockResponse);
  });
});
