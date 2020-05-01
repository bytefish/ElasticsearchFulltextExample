import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppComponent } from '@app/app.component';
import { FileUploadComponent } from '@app/components/file-upload/file-upload.component';
import { SearchComponent } from '@app/components/search/search.component';


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
    path: 'upload',
    component: FileUploadComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
