export interface EvaluateRequestDto {
  expression: string;
  mode: 'Infix' | 'Rpn';
  returnTrace: boolean;
  sessionId: string;
  settings?: EvaluateSettingsDto;
}

export interface EvaluateSettingsDto {
  precision?: number;
  rounding?: 'ToEven' | 'AwayFromZero' | 'ToZero' | 'ToNegativeInfinity' | 'ToPositiveInfinity';
}

export interface EvaluateResponseDto {
  result: string;
  mode: 'Infix' | 'Rpn';
  rpn: string[];
  trace: string[];
}

export interface KeyPressRequestDto {
  keys: string[];
  mode: 'Infix' | 'Rpn';
  returnTrace: boolean;
  sessionId: string;
  settings?: EvaluateSettingsDto;
}

export interface MemoryRequestDto {
  sessionId: string;
  command: 'MC' | 'MR' | 'MS' | 'MPlus' | 'MMinus';
  value?: number;
}

export interface ClearRequestDto {
  scope: 'CE' | 'C' | 'BACKSPACE';
}

export interface MemoryResponseDto {
  sessionId: string;
  value: number;
}
