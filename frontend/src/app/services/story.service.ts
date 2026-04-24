import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Story {
  id: number;
  title: string;
  url: string | null;
  score: number;
  commentCount: number;
  by: string | null;
  publishedAt: string;
}

export interface StoryResponse {
  stories: Story[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class StoryService {
  private apiUrl = `${environment.apiUrl}/api/stories`;

  constructor(private http: HttpClient) { }

  getStories(page: number = 1, pageSize: number = 30, search?: string, sortBy: string = 'date'): Observable<StoryResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy);

    if (search && search.trim()) {
      params = params.set('search', search);
    }

    return this.http.get<StoryResponse>(this.apiUrl, { params });
  }

  getStory(id: number): Observable<Story> {
    return this.http.get<Story>(`${this.apiUrl}/${id}`);
  }
}
