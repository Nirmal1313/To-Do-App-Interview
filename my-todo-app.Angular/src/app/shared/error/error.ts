import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-error',
  imports: [RouterLink, ButtonModule],
  templateUrl: './error.html',
  styleUrl: './error.scss',
})
export class ErrorComponent {
  constructor(private location: Location) {}

  goBack() {
    this.location.back();
  }
}
