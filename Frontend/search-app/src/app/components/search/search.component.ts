import { Component, ViewChildren, ElementRef, QueryList, ViewChild, OnInit, HostListener } from '@angular/core';
import { SearchResults, SearchSuggestions, SearchStateEnum, StatusEnum, SearchQuery } from '@app/app.model';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, empty, timer, zip, range, of, Subject, concat } from 'rxjs';
import { map, retryWhen, mergeMap, switchMap, debounceTime, filter, tap, catchError, finalize, delay, takeUntil } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { FileUploadComponent } from '@app/components/file-upload/file-upload.component';
import { MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { DocumentStatusComponent } from '../document-status/document-status.component';
import { SearchService } from '@app/services/search.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {
  control = new FormControl();

  query$: Observable<SearchQuery>;

  constructor(private route: ActivatedRoute, private httpClient: HttpClient, private searchService: SearchService) {

  }

  ngOnInit(): void {
    this.query$ = this.searchService.onSearchSubmit()
      .pipe(
        filter(query => !!query.term),
        switchMap(query =>
          concat(
            of(<SearchQuery>{ state: SearchStateEnum.Loading }),
            this.doSearch(query.term).pipe(
              map(results => <SearchQuery>{state: SearchStateEnum.Finished, data: results}),
              catchError(err => of(<SearchQuery>{ state: SearchStateEnum.Error, error: err }))
            )
          )
        )
      );
  }

  doSearch(query: string): Observable<SearchResults> {
    return this.httpClient
      .get<SearchResults>(`${environment.apiUrl}/search`, {
        params: {
          q: query
        }
      })
      .pipe(
        catchError(err => of(<SearchResults>{ query: query, results: [] }))
      );
  }
}
