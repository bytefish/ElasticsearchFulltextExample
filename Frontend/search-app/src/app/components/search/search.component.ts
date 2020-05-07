import { Component, ViewChildren, ElementRef, QueryList, ViewChild, OnInit, HostListener } from '@angular/core';
import { SearchResults, SearchSuggestions } from '@app/app.model';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, empty, timer, zip, range, of } from 'rxjs';
import { map, retryWhen, mergeMap, switchMap, debounceTime, filter, tap, catchError } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { FileUploadComponent } from '@app/components/file-upload/file-upload.component';
import { MatAutocompleteTrigger } from '@angular/material/autocomplete';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {

  control = new FormControl();

  results$: Observable<SearchResults>;
  suggestions$: Observable<SearchSuggestions>;

  @ViewChild('search', { read: MatAutocompleteTrigger }) 
  autoComplete: MatAutocompleteTrigger;

  constructor(private dialog: MatDialog, private router: Router, private route: ActivatedRoute, private httpClient: HttpClient) {

  }

  ngOnInit(): void {
    this.results$ = this.route.queryParams
      .pipe(
        map(params => params['q']),
        filter(query => !!query),
        tap(query => this.control.setValue(query, { emitEvent: false })),
        switchMap(query => this
          .doSearch(query)
          .pipe(catchError(err => of(<SearchResults>{ query: query, results: []}))))
      );

    this.suggestions$ = this.control.valueChanges
      .pipe(
        debounceTime(300), // Debounce time to not send every keystroke ...
        switchMap(value => this
          .getSuggestions(value)
          .pipe(catchError(err => of(<SearchSuggestions>{ query: value, results: []}))))
      );
  }

  onKeyupEnter(value: string): void {
    
    if(!!this.autoComplete) {
      this.autoComplete.closePanel();
    }

    // Instead of firing the Search directly, let's update the Route instead:
    this.router.navigate(['/search'], { queryParams: { q: value } });
  }

  doSearch(query: string): Observable<SearchResults> {

    return this.httpClient
      // Get the Results from the API:
      .get<SearchResults>(`${environment.apiUrl}/search`, {
        params: {
          q: query
        }
      })
      .pipe(catchError(err => of(<SearchResults>{ query: query, results: []})));
  }

  getSuggestions(query: string): Observable<SearchSuggestions> {

    if (!query) {
      return of(null);
    }

    if (query.length < 2) {
      return of(null);
    }

    return this.httpClient
      // Get the Results from the API:
      .get<SearchSuggestions>(`${environment.apiUrl}/suggest`, {
        params: {
          q: query
        }
      })
      .pipe(catchError(err => of(<SearchSuggestions>{ query: query, results: []})));
  }

  openFileUploadDocument() {
    this.dialog.open(FileUploadComponent);
  }
}
