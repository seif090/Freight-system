import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent {
  shipmentId = 0;
  file?: File;
  message = '';

  constructor(private http: HttpClient) {}

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.file = target.files[0];
    }
  }

  upload(): void {
    if (!this.file || !this.shipmentId) {
      this.message = 'اختر ملف وادخل رقم الشحنة.';
      return;
    }

    const formData = new FormData();
    formData.append('shipmentId', this.shipmentId.toString());
    formData.append('file', this.file);

    this.http.post('https://localhost:5001/api/v1.0/documents/upload', formData).subscribe({
      next: () => this.message = 'تم رفع المستند بنجاح',
      error: () => this.message = 'فشل رفع المستند'
    });
  }
}
