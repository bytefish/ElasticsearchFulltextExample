import { COMMA, ENTER } from '@angular/cdk/keycodes';

import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import {MatChipInputEvent} from '@angular/material/chips';

@Component({
    selector: 'app-fileupload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
    file: File;

    separatorKeysCodes: number[] = [ENTER, COMMA];
    
    fileUploadForm = new FormGroup({
        id: new FormControl('', Validators.required),
        title: new FormControl('', Validators.required),
        suggestions: new FormControl([], Validators.required),
        file: new FormControl('', Validators.required),
    });

    isFileUploading: boolean = false;

    constructor(public dialogRef: MatDialogRef<FileUploadComponent>, private httpClient: HttpClient) {

    }

    onFileInputChange(fileInputEvent: any) {
        this.file = fileInputEvent.target.files[0];

        this.fileUploadForm.controls['file'].setValue(this.file?.name);

        // Set ID as Filename, if empty:
        if (this.isNullOrWhitespace(this.fileUploadForm.controls['id'].value)) {
            this.fileUploadForm.controls['id'].setValue(this.file?.name);
        }
    }

    onAddSuggestion(event: MatChipInputEvent): void {
        const input = event.input;
        const value = event.value;
    
        if ((value || '').trim()) {
          this.fileUploadForm.controls['suggestions'].setErrors(null);
          this.suggestionsControl.value.push(value.trim());
        }
    
        if (input) {
          input.value = '';
        }

        this.suggestionsControl.updateValueAndValidity();
      }

      onRemoveSuggestion(suggestion: string): void {
        const index = this.suggestionsControl.value.indexOf(suggestion);
    
        if (index >= 0) {
            this.suggestionsControl.value.splice(index, 1);
        }

        this.suggestionsControl.updateValueAndValidity();
      }


    onSubmit() {

        if (this.fileUploadForm.invalid) {
            return;
        }

        this.isFileUploading = true;

        const formData = new FormData();

        formData.append('id', this.idControl.value);
        formData.append('title', this.titleControl.value);
        formData.append('suggestions', this.getCommaSeparatedSuggestions(this.suggestionsControl.value));
        formData.append('file', this.file);

        this.httpClient
            .put<any>(`${environment.apiUrl}/index`, formData)
            .subscribe(x => {
                this.isFileUploading = false;
                this.dialogRef.close();
            })
    }

    getCommaSeparatedSuggestions(values: string[]): string {
        return values
            .map(x => `"${x}"`)
            .join(",");
    }

    isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }


    get idControl() { 
        return this.fileUploadForm.get('id');
    }

    get titleControl() { 
        return this.fileUploadForm.get('title');
    }

    get suggestionsControl() { 
        return this.fileUploadForm.get('suggestions');
    }

    get fileControl() { 
        return this.fileUploadForm.get('file');
    }
}