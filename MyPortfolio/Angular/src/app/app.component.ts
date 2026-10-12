import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PortfolioPresentationService } from './core/services/portfolio-presentation.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly presentation = inject(PortfolioPresentationService);
  title = 'DevFolio Studio';

  ngOnInit(): void {
    this.presentation.loadPublicSettings();
  }
}
