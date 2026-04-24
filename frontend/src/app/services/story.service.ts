import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

interface Story {
  id: number;
  title: string;
  url: string;
  score: number;
  commentCount: number;
  publishedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class StoryService {
  private apiUrl = `${environment.apiUrl}/api/stories`;

  constructor(private http: HttpClient) { }

  getStories(): Observable<Story[]> {
    return this.http.get<Story[]>(this.apiUrl);
  }

  getStory(id: number): Observable<Story> {
    return this.http.get<Story>(`${this.apiUrl}/${id}`);
  }

  createStory(story: Omit<Story, 'id' | 'publishedAt'>): Observable<Story> {
    return this.http.post<Story>(this.apiUrl, story);
  }
}
