import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoryService } from './services/story.service';

interface Story {
  id: number;
  title: string;
  url: string;
  score: number;
  commentCount: number;
  publishedAt: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  stories: Story[] = [];
  loading = true;
  error: string | null = null;

  constructor(private storyService: StoryService) {}

  ngOnInit() {
    this.loadStories();
  }

  loadStories() {
    this.loading = true;
    this.storyService.getStories().subscribe({
      next: (data) => {
        this.stories = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load stories';
        this.loading = false;
        console.error('Error loading stories:', err);
      }
    });
  }
}
