import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConnectionStatusComponent } from './components/connection-status/connection-status.component';
import { OrderCreateFormComponent } from './components/order-create-form/order-create-form.component';
import { OrdersListComponent } from './components/orders-list/orders-list.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ConnectionStatusComponent, OrderCreateFormComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('orders-web');
}
