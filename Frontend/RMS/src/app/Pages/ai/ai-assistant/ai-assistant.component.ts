import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiAssistantService, ConversationMessage, ConversationSummary } from '../../../Services/ai-assistant.service';
import { ToastService } from '../../../Services/toast.service';

interface DisplayMessage {
  role: 'user' | 'assistant';
  content: string;
  toolsUsed?: string[];
}

const STARTER_QUESTIONS = [
  'Why did revenue change this week?',
  'Which menu items have high sales but low margins?',
  'Which items generate the most waste?',
  'What should be prepared tomorrow?',
  'Which ingredients may run out soon?',
  'What unusual changes happened this week?'
];

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-assistant.component.html',
  styleUrl: './ai-assistant.component.css'
})
export class AiAssistantComponent {
  starterQuestions = STARTER_QUESTIONS;
  conversations = signal<ConversationSummary[]>([]);
  activeConversationId = signal<number | null>(null);
  messages = signal<DisplayMessage[]>([]);
  question = signal('');
  asking = signal(false);
  loadingHistory = signal(false);

  constructor(private service: AiAssistantService, private toast: ToastService) {
    this.loadConversations();
  }

  loadConversations() {
    this.service.getConversations().subscribe({
      next: (list) => this.conversations.set(list),
      error: () => this.toast.error('Failed to load conversation history')
    });
  }

  openConversation(id: number) {
    this.loadingHistory.set(true);
    this.service.getConversation(id).subscribe({
      next: (detail) => {
        this.activeConversationId.set(detail.id);
        this.messages.set(detail.messages.map((m: ConversationMessage) => ({ role: m.role, content: m.content })));
        this.loadingHistory.set(false);
      },
      error: () => {
        this.toast.error('Failed to load conversation');
        this.loadingHistory.set(false);
      }
    });
  }

  newConversation() {
    this.activeConversationId.set(null);
    this.messages.set([]);
  }

  setQuestion(value: string) {
    this.question.set(value);
  }

  askStarter(q: string) {
    this.question.set(q);
    this.ask();
  }

  ask() {
    const q = this.question().trim();
    if (!q || this.asking()) return;

    this.messages.update(list => [...list, { role: 'user', content: q }]);
    this.question.set('');
    this.asking.set(true);

    this.service.ask(q, this.activeConversationId() ?? undefined).subscribe({
      next: (res) => {
        this.activeConversationId.set(res.conversationId);
        this.messages.update(list => [...list, { role: 'assistant', content: res.answer, toolsUsed: res.toolsUsed }]);
        this.asking.set(false);
        this.loadConversations();
      },
      error: (err) => {
        this.asking.set(false);
        const message = err?.error?.message || 'The assistant could not answer that question. Please try again.';
        this.messages.update(list => [...list, { role: 'assistant', content: message }]);
      }
    });
  }
}
