import { COMMA, ENTER } from '@angular/cdk/keycodes';

import { Component, AfterViewInit, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { FormControl, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatChipInputEvent } from '@angular/material/chips';
import { StringUtils } from '@app/utils/string-utils';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { DocumentStatus } from '@app/app.model';
import { catchError } from 'rxjs/operators';
import { observable, of } from 'rxjs';

@Component({
    selector: 'app-document-status',
    templateUrl: './document-status.component.html',
    styleUrls: ['./document-status.component.scss']
})
export class DocumentStatusComponent implements OnInit {

    displayedColumns: string[] = ['select', 'documentId', 'title', 'filename', 'isOcrRequested', 'status'];

    isDataSourceLoading: boolean = false;
    dataSource = new MatTableDataSource<DocumentStatus>();
    selection = new SelectionModel<DocumentStatus>(true, []);
  
    constructor(private httpClient: HttpClient) {

    }

    ngOnInit(): void {
        this.reloadDataTable();
    }

    reloadDataTable() {
        this.isDataSourceLoading = true;

        this.httpClient
            .get<DocumentStatus[]>(`${environment.apiUrl}/status`)
            .pipe(catchError(() => of<DocumentStatus[]>([])))
            .subscribe(data => {
                this.isDataSourceLoading = false;
                this.dataSource.data = data;
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
      return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.documentId}`;
    }
}