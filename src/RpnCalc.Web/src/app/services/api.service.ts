import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  EvaluateRequestDto,
  EvaluateResponseDto,
  KeyPressRequestDto,
  MemoryRequestDto,
  MemoryResponseDto,
  ClearRequestDto
} from '../models';

const SESSION_KEY = 'rpn-session-id';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = '/api/v1';

  constructor(private readonly http: HttpClient) {}

  get sessionId(): string {
    const cached = localStorage.getItem(SESSION_KEY);
    if (cached) {
      return cached;
    }

    const session = crypto.randomUUID();
    localStorage.setItem(SESSION_KEY, session);
    return session;
  }

  evaluate(request: Omit<EvaluateRequestDto, 'sessionId'>): Observable<EvaluateResponseDto> {
    return this.http.post<EvaluateResponseDto>(`${this.baseUrl}/calc/evaluate`, {
      ...request,
      sessionId: this.sessionId
    });
  }

  press(request: Omit<KeyPressRequestDto, 'sessionId'>): Observable<EvaluateResponseDto> {
    return this.http.post<EvaluateResponseDto>(`${this.baseUrl}/calc/press`, {
      ...request,
      sessionId: this.sessionId
    });
  }

  getMemory(): Observable<MemoryResponseDto> {
    return this.http.get<MemoryResponseDto>(`${this.baseUrl}/memory`, {
      params: { sessionId: this.sessionId }
    });
  }

  applyMemory(command: MemoryRequestDto): Observable<MemoryResponseDto> {
    return this.http.post<MemoryResponseDto>(`${this.baseUrl}/memory`, command);
  }

  clear(request: ClearRequestDto): Observable<string> {
    return this.http.post<{ cleared: string }>(`${this.baseUrl}/clear`, request).pipe(map((x) => x.cleared));
  }
}
