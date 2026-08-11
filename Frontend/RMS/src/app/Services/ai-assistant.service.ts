import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface AskResponse {
  conversationId: number;
  answer: string;
  toolsUsed: string[];
}

export interface ConversationSummary {
  id: number;
  title: string | null;
  createdAt: string;
  lastMessageAt: string;
}

export interface ConversationMessage {
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface ConversationDetail {
  id: number;
  title: string | null;
  messages: ConversationMessage[];
}

@Injectable({ providedIn: 'root' })
export class AiAssistantService {
  private api = environment.apiBaseUrl + '/ai/assistant';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.auth.gettoken() });
  }

  ask(question: string, conversationId?: number): Observable<AskResponse> {
    return this.http.post<AskResponse>(`${this.api}/ask`, { question, conversationId }, { headers: this.headers() });
  }

  getConversations(): Observable<ConversationSummary[]> {
    return this.http.get<ConversationSummary[]>(`${this.api}/conversations`, { headers: this.headers() });
  }

  getConversation(id: number): Observable<ConversationDetail> {
    return this.http.get<ConversationDetail>(`${this.api}/conversations/${id}`, { headers: this.headers() });
  }
}
