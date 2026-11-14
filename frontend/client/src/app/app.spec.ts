import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { App } from './app';
import { AuthService } from './auth.service';
import { ManagementData, ManagementService } from './management.service';

const testManagementData: ManagementData = {
  users: [
    { id: 1, firstName: 'Tebza', lastName: 'Customer', userName: 'customer', email: 'customer@example.com', roles: [] },
    { id: 2, firstName: 'Wash', lastName: 'Team', userName: 'washer', email: 'washer@example.com', roles: [] },
  ],
  customers: [{ id: 1, userId: 1, firstName: 'Tebza', lastName: 'Customer' }],
  washers: [{ id: 1, userId: 2, isAvailable: true, hiredOn: '2026-01-01T00:00:00Z', firstName: 'Wash', lastName: 'Team' }],
  bookings: [{ id: 1, customerId: 1, vehicleId: 1, serviceId: 1, washerId: 1, scheduledAt: '2026-11-15T10:00:00Z', status: 'Requested' }],
  vehicles: [{ id: 1, customerId: 1, registration: 'ABC123', make: 'Toyota', model: 'Corolla', colour: 'Blue', type: 'Sedan' }],
  services: [{ id: 1, name: 'Basic wash', description: null, price: 100, durationMinutes: 30 }],
  washes: [{ id: 1, bookingId: 1, customerId: 1, vehicleId: 1, serviceId: 1, washerId: 1, price: 100, isPaid: false, status: 'Pending', startedAt: '2026-11-15T10:00:00Z', completedAt: null }],
};

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        { provide: ManagementService, useValue: { loadAll: () => Promise.resolve(testManagementData) } },
      ],
    })
      .compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the sign-in form', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Sign in to your account');
    expect(compiled.querySelector('input[name="userName"]')).toBeTruthy();
    expect(compiled.textContent).not.toContain('Clean cars.');
  });

  it('should show registration fields when creating an account', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const createAccountButton = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((button) => button.textContent?.includes('Create an account'));
    createAccountButton?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Create your account');
    expect(compiled.querySelector('input[name="email"]')).toBeTruthy();
    expect(compiled.querySelector('input[name="confirmPassword"]')).toBeTruthy();
  });

  it('should show the management menu after authentication', async () => {
    const fixture = TestBed.createComponent(App);
    TestBed.inject(AuthService).currentUser.set({
      id: 1,
      firstName: 'Thabo',
      lastName: 'Mokoena',
      userName: 'thabo',
      email: 'thabo@example.com',
    });
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.dashboard-shell')).toBeTruthy();
    for (const section of ['Overview', 'Customers', 'Washers', 'Bookings', 'Vehicles', 'Services', 'Washes']) {
      expect(compiled.textContent).toContain(section);
    }

    const vehiclesButton = Array.from(compiled.querySelectorAll<HTMLButtonElement>('nav button')).find((button) =>
      button.textContent?.includes('Vehicles'),
    );
    vehiclesButton?.click();
    fixture.detectChanges();
    expect(compiled.textContent).toContain('Vehicles directory');
  });

  it('should offer create and edit forms for every management section', async () => {
    const fixture = TestBed.createComponent(App);
    TestBed.inject(AuthService).currentUser.set({
      id: 1,
      firstName: 'Thabo',
      lastName: 'Mokoena',
      userName: 'thabo',
      email: 'thabo@example.com',
    });
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const sections = [
      ['Customers', 'Customer'],
      ['Washers', 'Washer'],
      ['Bookings', 'Booking'],
      ['Vehicles', 'Vehicle'],
      ['Services', 'Service'],
      ['Washes', 'Wash'],
    ];

    for (const [section, recordName] of sections) {
      const navButton = Array.from(compiled.querySelectorAll<HTMLButtonElement>('.dashboard-nav button'))
        .find((button) => button.textContent?.includes(section));
      navButton?.click();
      fixture.detectChanges();

      expect(compiled.querySelector('.add-record-button')).toBeTruthy();
      const editButton = compiled.querySelector<HTMLButtonElement>('.row-action');
      editButton?.click();
      fixture.detectChanges();
      expect(compiled.querySelector('#record-modal-title')?.textContent).toContain(`Edit ${recordName}`);
      compiled.querySelector<HTMLButtonElement>('.modal-close')?.click();
      fixture.detectChanges();

      compiled.querySelector<HTMLButtonElement>('.add-record-button')?.click();
      fixture.detectChanges();
      expect(compiled.querySelector('#record-modal-title')?.textContent).toContain(`Add ${recordName}`);
      compiled.querySelector<HTMLButtonElement>('.modal-close')?.click();
      fixture.detectChanges();
    }
  });
});
