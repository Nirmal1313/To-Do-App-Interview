import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Leftnav } from './leftnav';

describe('Leftnav', () => {
  let component: Leftnav;
  let fixture: ComponentFixture<Leftnav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Leftnav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Leftnav);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
