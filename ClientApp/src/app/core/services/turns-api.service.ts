import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiHttpService } from './api-http.service';
import { TurnCreateRequest, TurnDto, TurnUpdateRequest } from '../models/turn.model';

@Injectable({ providedIn: 'root' })
export class TurnsApiService {
  private readonly api = inject(ApiHttpService);

  getTurns(params: { date?: string; medicId?: string; includeAccessed?: boolean } = {}): Observable<TurnDto[]> {
    return this.api.get<TurnDto[]>('/turns', params);
  }

  createTurn(payload: TurnCreateRequest): Observable<void> {
    return this.api.post<void>('/turns', payload);
  }

  updateTurn(id: string, payload: TurnUpdateRequest): Observable<void> {
    return this.api.put<void>(`/turns/${id}`, payload);
  }

  deleteTurn(id: string): Observable<void> {
    return this.api.delete<void>(`/turns/${id}`);
  }

  markAsAccessed(id: string): Observable<void> {
    return this.api.post<void>(`/turns/${id}/accessed`, {});
  }
}
