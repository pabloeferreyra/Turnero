import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiHttpService } from './api-http.service';
import { MedicDto } from '../models/medic.model';

@Injectable({ providedIn: 'root' })
export class MedicsApiService {
  private readonly api = inject(ApiHttpService);

  getMedics(): Observable<MedicDto[]> {
    return this.api.get<MedicDto[]>('/medics');
  }
}
