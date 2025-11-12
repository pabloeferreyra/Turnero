import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiHttpService } from './api-http.service';
import { TimeTurnDto } from '../models/time-turn.model';

@Injectable({ providedIn: 'root' })
export class TimeTurnsApiService {
  private readonly api = inject(ApiHttpService);

  getTimeTurns(): Observable<TimeTurnDto[]> {
    return this.api.get<TimeTurnDto[]>('/time-turns');
  }
}
