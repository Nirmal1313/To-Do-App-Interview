import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Rightnav } from './rightnav';

describe('Rightnav', () => {
  let component: Rightnav;
  let fixture: ComponentFixture<Rightnav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Rightnav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Rightnav);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
