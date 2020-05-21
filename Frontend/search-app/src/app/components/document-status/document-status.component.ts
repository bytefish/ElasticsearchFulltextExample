// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Component, OnInit, HostListener, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { DocumentStatus } from '@app/app.model';
import { catchError, concatMap, mergeMap, toArray, tap, takeUntil } from 'rxjs/operators';
import { of, from, Subject, interval } from 'rxjs';

@Component({
  selector: 'app-document-status',
  templateUrl: './document-status.component.html',
  styleUrls: ['./document-status.component.scss']
})
export class DocumentStatusComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  displayedColumns: string[] = ['select', 'id', 'title', 'filename', 'isOcrRequested', 'status'];

  isDataSourceLoading: boolean = false;
  dataSource = new MatTableDataSource<DocumentStatus>();
  selection = new SelectionModel<DocumentStatus>(true, []);

  constructor(private httpClient: HttpClient, private changeDetectorRefs: ChangeDetectorRef) {

  }

  ngOnInit(): void {
    interval(5000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.reloadStatusValues());

    this.reloadDataTable();
  }

  reloadDataTable() {
    this.selection.clear();

    this.httpClient
      .get<DocumentStatus[]>(`${environment.apiUrl}/status`)
      .pipe(
        catchError(() => of<DocumentStatus[]>([])))
      .subscribe(data => {
        this.dataSource.data = data;
      });
  }

  reloadStatusValues() {
    this.httpClient
      .get<DocumentStatus[]>(`${environment.apiUrl}/status`)
      .pipe(
        catchError(() => of<DocumentStatus[]>([])))
      .subscribe(data => {

        const status = new Map(data.map(i => [i.id, i.status]));

        this.dataSource.data
          .forEach(row => {
            if (status.has(row.id)) {
              row.status = status.get(row.id);
            }
          });

        this.changeDetectorRefs.detectChanges();
      });
  }


  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  /** The label for the checkbox on the passed row */
  checkboxLabel(row?: DocumentStatus): string {
    if (!row) {
      return `${this.isAllSelected() ? 'select' : 'deselect'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id}`;
  }

  @HostListener('document:keyup', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.altKey && event.key === 'Delete') {
      this.removeSelectedDocuments();
    }

    if (event.altKey && (event.key === 'r' || event.key === 'R')) {
      this.scheduleSelectedDocuments();
    }
  }

  removeSelectedDocuments() {

    var documentsToRemove = this.selection.selected

    from(documentsToRemove)
      .pipe(
        mergeMap(x => this.httpClient.delete(`${environment.apiUrl}/status/${x.id}`)),
        toArray()
      )
      .subscribe(() => this.reloadDataTable());
  }

  scheduleSelectedDocuments() {

    var documentsToIndex = this.selection.selected

    from(documentsToIndex)
      .pipe(
        mergeMap(x => this.httpClient.post<any>(`${environment.apiUrl}/status/${x.id}/index`, [])),
        toArray()
      )
      .subscribe(() => this.reloadDataTable());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}