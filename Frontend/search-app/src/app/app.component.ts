import { Component, ViewChild } from '@angular/core';
import { SearchSuggestions } from '@app/app.model';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { switchMap, debounceTime, catchError } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { DocumentStatusComponent } from './components/document-status/document-status.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  control = new FormControl();

  suggestions$: Observable<SearchSuggestions>;

  @ViewChild('search', { read: MatAutocompleteTrigger }) 
  autoComplete: MatAutocompleteTrigger;

  constructor(private dialog: MatDialog, private router: Router, private httpClient: HttpClient) {

  }

  ngOnInit(): void {
    this.suggestions$ = this.control.valueChanges
      .pipe(
        debounceTime(300), // Debounce time to not send every keystroke ...
        switchMap(value => this
          .getSuggestions(value)
          .pipe(catchError(() => of(<SearchSuggestions>{ query: value, results: []}))))
      );
  }

  onKeyupEnter(value: string): void {
    
    if(!!this.autoComplete) {
      this.autoComplete.closePanel();
    }

    // Instead of firing the Search directly, let's update the Route instead:
    this.router.navigate(['/search'], { queryParams: { q: value } });
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
      .pipe(catchError(() => of(<SearchSuggestions>{ query: query, results: []})));
  }

  openFileUploadDialog() {
    this.dialog.open(FileUploadComponent);
  }

  openDocumentStatusDialog() {
    this.dialog.open(DocumentStatusComponent);
  }
}
