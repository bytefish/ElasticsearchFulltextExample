// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { FormControl, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatChipInputEvent } from '@angular/material/chips';
import { StringUtils } from '@app/utils/string-utils';

@Component({
    selector: 'app-fileupload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
    file: File;

    separatorKeysCodes: number[] = [ENTER, COMMA];

    fileUploadForm = new FormGroup({
        title: new FormControl('', Validators.required),
        suggestions: new FormControl([], Validators.required),
        file: new FormControl('', Validators.required),
        ocr: new FormControl(false)
    });

    isFileUploading: boolean = false;

    constructor(public dialogRef: MatDialogRef<FileUploadComponent>, private httpClient: HttpClient) {

    }

    onFileInputChange(fileInputEvent: any): void {
        this.file = fileInputEvent.target.files[0];
        this.fileControl.setValue(this.file?.name);
    }

    onAddSuggestion(event: MatChipInputEvent): void {

        const input = event.input;
        const value = event.value;

        if (!StringUtils.isNullOrWhitespace(value)) {
            this.suggestionsControl.setErrors(null);
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


    onSubmit(): void {

        if (this.fileUploadForm.invalid) {
            return;
        }

        this.isFileUploading = true;

        this.httpClient
            .post<any>(`${environment.apiUrl}/index`, this.buildRequestFormData())
            .subscribe(x => {
                this.isFileUploading = false;
                this.dialogRef.close();
            })
    }

    buildRequestFormData(): FormData {
        const formData = new FormData();

        formData.append('title', this.titleControl.value);
        formData.append('suggestions', this.getCommaSeparatedSuggestions(this.suggestionsControl.value));
        formData.append('file', this.file);
        formData.append('isOcrRequested', this.ocrControl.value);

        return formData;
    }

    getCommaSeparatedSuggestions(values: string[]): string {
        return values
            .map(x => `"${x}"`)
            .join(",");
    }

    get titleControl(): AbstractControl {
        return this.fileUploadForm.get('title');
    }

    get suggestionsControl(): AbstractControl {
        return this.fileUploadForm.get('suggestions');
    }

    get fileControl(): AbstractControl {
        return this.fileUploadForm.get('file');
    }

    get ocrControl(): AbstractControl {
        return this.fileUploadForm.get('ocr');
    }
}