import { Component, HostListener, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { EvaluateResponseDto, MemoryRequestDto, MemoryResponseDto } from '../../models';

type Mode = 'Infix' | 'Rpn';

type MemoryButton = 'MC' | 'MR' | 'MS' | 'MPlus' | 'MMinus';

type KeyButton = string;

@Component({
  selector: 'app-calculator',
  templateUrl: './calculator.component.html',
  styleUrls: ['./calculator.component.scss']
})
export class CalculatorComponent implements OnInit {
  mode: Mode = 'Infix';
  display = '0';
  rpn: string[] = [];
  trace: string[] = [];
  error?: string;
  memory = 0;
  precision = 15;
  private keySequence: KeyButton[] = [];

  readonly digits = ['7', '8', '9', '4', '5', '6', '1', '2', '3', '0', '.'];
  readonly operators = ['+', '-', '*', '/', '^'];
  readonly infixExtras = ['(', ')'];
  readonly controls: KeyButton[] = ['CE', 'C', 'BACKSPACE', '='];
  readonly memoryButtons: MemoryButton[] = ['MC', 'MR', 'MS', 'MPlus', 'MMinus'];

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadMemory();
  }

  toggleMode(mode: Mode): void {
    this.mode = mode;
    this.resetSequence();
    this.display = '0';
    this.rpn = [];
    this.trace = [];
    this.error = undefined;
  }

  onDigitPress(value: KeyButton): void {
    this.enqueue(value);
  }

  onOperatorPress(value: KeyButton): void {
    this.enqueue(value);
  }

  onControlPress(value: KeyButton): void {
    if (value === 'C') {
      this.resetSequence();
    }

    this.enqueue(value);
  }

  onMemoryPress(button: MemoryButton): void {
    const payload: MemoryRequestDto = {
      sessionId: this.api.sessionId,
      command: button,
      value: this.displayAsNumber(button)
    };

    this.api.applyMemory(payload).subscribe({
      next: (response: MemoryResponseDto) => {
        this.memory = response.value;
        if (button === 'MR') {
          this.display = response.value.toString();
        }
      },
      error: (err: unknown) => this.showError(err)
    });
  }

  @HostListener('document:keydown', ['$event'])
  handleKey(event: KeyboardEvent): void {
    const map: Record<string, KeyButton> = {
      Enter: '=',
      Escape: 'CE',
      Backspace: 'BACKSPACE'
    };

    if (this.digits.includes(event.key)) {
      this.onDigitPress(event.key);
      return;
    }

    if (this.operators.includes(event.key)) {
      this.onOperatorPress(event.key);
      return;
    }

    const mapped = map[event.key];
    if (mapped) {
      this.onControlPress(mapped);
    }
  }

  private enqueue(value: KeyButton): void {
    this.error = undefined;
    this.keySequence.push(value);
    const request = {
      keys: this.keySequence,
      mode: this.mode,
      returnTrace: true,
      settings: { precision: this.precision }
    };

    this.api.press(request).subscribe({
      next: (response: EvaluateResponseDto) => this.applyResponse(response),
      error: (err: unknown) => this.showError(err)
    });

    if (value === 'C' || value === 'CE') {
      this.resetSequence();
    }

    if (value === '=') {
      this.keySequence = [];
    }
  }

  private applyResponse(response: EvaluateResponseDto): void {
    this.display = response.result;
    this.rpn = response.rpn ?? [];
    this.trace = response.trace ?? [];
    this.loadMemory();
  }

  private showError(error: unknown): void {
    if (error && typeof error === 'object' && 'error' in error) {
      const candidate = error as { error?: { detail?: string } };
      this.error = candidate.error?.detail ?? 'Unexpected error';
      return;
    }

    this.error = 'Unexpected error';
  }

  private displayAsNumber(button: MemoryButton): number | undefined {
    if (button === 'MS' || button === 'MPlus' || button === 'MMinus') {
      return Number(this.display);
    }

    return undefined;
  }

  private resetSequence(): void {
    this.keySequence = [];
  }

  private loadMemory(): void {
    this.api.getMemory().subscribe({
      next: (response: MemoryResponseDto) => {
        this.memory = response.value;
      },
      error: () => {
        this.memory = 0;
      }
    });
  }
}
