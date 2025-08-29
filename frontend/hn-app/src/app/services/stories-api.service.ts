import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../tokens/api-base-url.token';
import { PagedStories } from '../models/story';

@Injectable({
  providedIn: 'root'
})
export class StoriesApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  getStories(pageIndex: number, pageSize: number, search?: string): Observable<PagedStories> {
    const offset = pageIndex * pageSize;
    let url = `${this.apiBaseUrl}/api/stories/new?offset=${offset}&limit=${pageSize}`;
    
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }

    return this.httpClient.get<PagedStories>(url);
  }
}
