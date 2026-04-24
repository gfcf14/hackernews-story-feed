import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { StoryService } from './story.service';
import { environment } from '../../environments/environment';

describe('StoryService', () => {
  let service: StoryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [StoryService]
    });

    service = TestBed.inject(StoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should fetch stories with query params', () => {
    service.getStories(2, 30, 'test', 'score').subscribe();

    const req = httpMock.expectOne((r) =>
      r.url === `${environment.apiUrl}/api/stories` &&
      r.params.get('page') === '2' &&
      r.params.get('pageSize') === '30' &&
      r.params.get('search') === 'test' &&
      r.params.get('sortBy') === 'score'
    );

    expect(req.request.method).toBe('GET');
    req.flush({ stories: [], totalCount: 0, page: 2, pageSize: 30, totalPages: 0 });
  });

  it('should omit search param when empty', () => {
    service.getStories(1, 30, '', 'date').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/stories?page=1&pageSize=30&sortBy=date`);
    expect(req.request.method).toBe('GET');

    req.flush({ stories: [], totalCount: 0, page: 1, pageSize: 30, totalPages: 0 });
  });
});
