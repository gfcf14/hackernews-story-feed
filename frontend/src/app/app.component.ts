import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StoryService, Story, StoryResponse } from './services/story.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  Math = Math;
  stories: Story[] = [];
  loading = true;
  error: string | null = null;
  
  // Pagination
  currentPage = 1;
  pageSize = 30;
  totalPages = 0;
  totalCount = 0;
  
  // Search and sorting
  searchQuery = '';
  sortBy: 'date' | 'score' = 'date';

  constructor(private storyService: StoryService) {}

  ngOnInit() {
    this.loadStories();
  }

  loadStories() {
    this.loading = true;
    this.error = null;
    this.storyService.getStories(this.currentPage, this.pageSize, this.searchQuery, this.sortBy).subscribe({
      next: (response: StoryResponse) => {
        this.stories = response.stories ?? [];
        this.totalPages = response.totalPages ?? 0;
        this.totalCount = response.totalCount ?? 0;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load stories. Please try again later.';
        this.loading = false;
        console.error('Error loading stories:', err);
      }
    });
  }

  onSearch(query: string) {
    this.searchQuery = query;
    this.currentPage = 1; // Reset to first page when searching
    this.loadStories();
  }

  onSortChange(sortBy: 'date' | 'score') {
    this.sortBy = sortBy;
    this.currentPage = 1; // Reset to first page when changing sort
    this.loadStories();
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;

    this.currentPage = page;
    this.loadStories();
    window.scrollTo(0, 0); // Scroll to top
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);
    
    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    
    return date.toLocaleDateString();
  }

  hasUrl(story: Story): boolean {
    return story.url != null && story.url.trim() !== '';
  }
}
