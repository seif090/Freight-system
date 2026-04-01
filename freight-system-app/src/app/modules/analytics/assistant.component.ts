import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assistant.component.html'
})
export class AssistantComponent {
  userInput = '';
  conversation: any[] = [];

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  private hashContext(input: string): string {
    let hash = 0;
    if (input.length === 0) return hash.toString();
    for (let i = 0; i < input.length; i++) {
      const chr = input.charCodeAt(i);
      hash = ((hash << 5) - hash) + chr;
      hash |= 0;
    }
    return hash.toString();
  }

  send(): void {
    const contextText = this.conversation.map(x => x.text).join(' ');
    const request = {
      userId: 'current-user',
      input: this.userInput,
      contextHash: this.hashContext(contextText)
    };

    this.conversation.unshift({ role: 'user', text: this.userInput });

    this.analyticsService.executeAssistant(request).subscribe((res: any) => {
      this.conversation.unshift({ role: 'assistant', text: res.results });
      this.userInput = '';
    });
  }
}