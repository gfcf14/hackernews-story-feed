import { TestBed, ComponentFixture } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { StoryService } from './services/story.service';
import { of } from 'rxjs';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let mockService: jasmine.SpyObj<StoryService>;

  beforeEach(async () => {
    mockService = jasmine.createSpyObj('StoryService', ['getStories']);

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [{ provide: StoryService, useValue: mockService }]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
  });

  it('should load stories on init', () => {
    mockService.getStories.and.returnValue(
      of({
        stories: [
          { id: 1, title: 'Test', url: null, score: 10, commentCount: 0, by: 'me', publishedAt: new Date().toISOString() }
        ],
        totalCount: 1,
        page: 1,
        pageSize: 30,
        totalPages: 1
      })
    );

    fixture.componentInstance.ngOnInit();

    expect(mockService.getStories).toHaveBeenCalled();
  });

  it('should handle empty results', () => {
    mockService.getStories.and.returnValue(
      of({ stories: [], totalCount: 0, page: 1, pageSize: 30, totalPages: 0 })
    );

    fixture.componentInstance.loadStories();

    expect(fixture.componentInstance.stories.length).toBe(0);
  });

  it('should paginate correctly', () => {
    const comp = fixture.componentInstance;

    comp.totalPages = 10;
    comp.currentPage = 5;

    const pages = comp.getPageNumbers();

    expect(pages.length).toBeGreaterThan(0);
    expect(pages).toContain(5);
  });
});
