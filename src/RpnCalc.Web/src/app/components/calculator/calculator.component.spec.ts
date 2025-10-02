import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { CalculatorComponent } from './calculator.component';
import { ApiService } from '../../services/api.service';
import { of } from 'rxjs';

describe('CalculatorComponent', () => {
  let component: CalculatorComponent;
  let fixture: ComponentFixture<CalculatorComponent>;
  let apiService: jasmine.SpyObj<ApiService>;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj<ApiService>('ApiService', ['press', 'applyMemory', 'getMemory'], {
      sessionId: 'test-session'
    });
    apiService.press.and.returnValue(of({ result: '0', mode: 'Infix', rpn: [], trace: [] }));
    apiService.applyMemory.and.returnValue(of({ sessionId: 'test', value: 0 }));
    apiService.getMemory.and.returnValue(of({ sessionId: 'test', value: 0 }));

    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, FormsModule],
      declarations: [CalculatorComponent],
      providers: [{ provide: ApiService, useValue: apiService }]
    }).compileComponents();

    fixture = TestBed.createComponent(CalculatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should enqueue digits and request evaluation', () => {
    component.onDigitPress('1');
    expect(apiService.press).toHaveBeenCalled();
  });
});
