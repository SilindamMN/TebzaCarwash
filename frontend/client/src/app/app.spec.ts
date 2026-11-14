import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { App } from './app';
import { AuthService } from './auth.service';
import { ManagementData, ManagementService } from './management.service';

const emptyManagementData: ManagementData = {
  customers: [],
  washers: [],
  bookings: [],
  vehicles: [],
  services: [],
  washes: [],
};

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        { provide: ManagementService, useValue: { loadAll: () => Promise.resolve(emptyManagementData) } },
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
});
