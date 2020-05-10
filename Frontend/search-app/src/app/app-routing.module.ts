import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppComponent } from '@app/app.component';
import { FileUploadComponent } from '@app/components/file-upload/file-upload.component';
import { SearchComponent } from '@app/components/search/search.component';
import { DocumentStatusComponent } from './components/document-status/document-status.component';


const routes: Routes = [
  { path: '',
    pathMatch: 'full',
    redirectTo: "search"
  },
  {
    path: 'search',
    component: SearchComponent
  },
  {
    path: 'status',
    component: DocumentStatusComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
