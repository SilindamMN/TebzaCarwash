import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { AuthService } from './auth.service';
import { Dashboard } from './dashboard';
import {
  BookingRecord,
  CustomerRecord,
  ManagementData,
  ManagementPayload,
  ManagementSection,
  ManagementService,
  ServiceRecord,
  VehicleRecord,
  WasherRecord,
  WashRecord,
} from './management.service';

describe('Dashboard create and update flows', () => {
  let data: ManagementData;
  let calls: Array<{ section: ManagementSection; id: number | null; payload: ManagementPayload }>;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    data = {
      users: [
        { id: 10, firstName: 'Ava', lastName: 'Customer', userName: 'ava', email: 'ava@example.com', roles: ['CUSTOMER'] },
        { id: 11, firstName: 'Wes', lastName: 'Washer', userName: 'wes', email: 'wes@example.com', roles: ['WASHER'] },
        { id: 12, firstName: 'Sam', lastName: 'Spare', userName: 'sam', email: 'sam@example.com', roles: [] },
        { id: 13, firstName: 'Sipho', lastName: 'Washer', userName: 'sipho', email: 'sipho@example.com', roles: ['WASHER'] },
      ],
      customers: [],
      washers: [],
      bookings: [],
      vehicles: [],
      services: [],
      washes: [],
    };
    calls = [];

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        {
          provide: AuthService,
          useValue: {
            accessToken: 'test-token',
            currentUser: signal({
              id: 99,
              firstName: 'Test',
              lastName: 'Manager',
              userName: 'manager',
              email: 'manager@example.com',
            }),
            signOut: () => undefined,
            getErrorMessage: () => 'Save failed.',
          },
        },
        {
          provide: ManagementService,
          useValue: {
            loadAll: async () => data,
            saveRecord: async (
              _accessToken: string,
              section: ManagementSection,
              id: number | null,
              payload: ManagementPayload,
            ) => {
              calls.push({ section, id, payload });
              const recordId = id ?? data[section].length + 1;
              applyPayload(section, recordId, payload);
            },
          },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    await settle();
  });

  it('creates and updates customers, washers, vehicles, services, bookings, and washes', async () => {
    await chooseSection('Customers');
    await click('.add-record-button');
    await selectValue('select[name="customerUser"]', 'ava');
    await submit('Create record');

    await chooseSection('Washers');
    await click('.add-record-button');
    await selectValue('select[name="washerUser"]', 'wes');
    await submit('Create record');

    await chooseSection('Washers');
    await click('.add-record-button');
    await selectValue('select[name="washerUser"]', 'sipho');
    await setChecked('input[name="washerAvailable"]', false);
    await submit('Create record');

    await chooseSection('Vehicles');
    await click('.add-record-button');
    expect(optionTexts('select[name="vehicleCustomer"]').join(' ')).not.toContain('Wes');
    expect(optionTexts('select[name="vehicleCustomer"]').join(' ')).not.toContain('Sipho');
    await selectValue('select[name="vehicleCustomer"]', 'Ava Customer');
    await setValue('input[name="vehicleRegistration"]', 'ABC123');
    await setValue('input[name="vehicleMake"]', 'Toyota');
    await setValue('input[name="vehicleModel"]', 'Corolla');
    await submit('Create record');

    await chooseSection('Services');
    await click('.add-record-button');
    await setValue('input[name="serviceName"]', 'Basic wash');
    await setValue('input[name="servicePrice"]', '100');
    await submit('Create record');

    await chooseSection('Bookings');
    await click('.add-record-button');
    await selectValue('select[name="bookingCustomer"]', 'Ava Customer');
    await selectValue('select[name="bookingVehicle"]', 'ABC123');
    await selectValue('select[name="bookingService"]', 'Basic wash');
    expect(bookingWasherDisabled()).toBe(true);
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T08:59');
    const scheduleInput = (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLInputElement>('input[name="bookingScheduledAt"]');
    expect(scheduleInput?.min).toBe('2026-11-15T09:00');
    expect(scheduleInput?.max).toBe('2026-11-15T20:30');
    expect((fixture.nativeElement as HTMLElement).querySelector('.booking-schedule-error')?.textContent)
      .toContain('09:00');
    expect(bookingWasherDisabled()).toBe(true);
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T10:00');
    expect(bookingWasherDisabled()).toBe(false);
    expect(optionTexts('select[name="bookingWasher"]').join(' ')).toContain('Wes');
    expect(optionTexts('select[name="bookingWasher"]').join(' ')).not.toContain('Sipho');
    await selectValue('select[name="bookingWasher"]', 'Wes Washer');
    await submit('Create record');

    await click('.add-record-button');
    await selectValue('select[name="bookingCustomer"]', 'Ava Customer');
    await selectValue('select[name="bookingVehicle"]', 'ABC123');
    await selectValue('select[name="bookingService"]', 'Basic wash');
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T10:15');
    expect(optionTexts('select[name="bookingWasher"]').join(' ')).not.toContain('Wes');
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T20:45');
    expect((fixture.nativeElement as HTMLElement).querySelector('.booking-schedule-error')?.textContent)
      .toContain('21:00');
    expect(bookingWasherDisabled()).toBe(true);
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T20:30');
    expect(fixture.nativeElement.querySelector('.booking-schedule-error')).toBeNull();
    expect(bookingWasherDisabled()).toBe(false);
    await setValue('input[name="bookingScheduledAt"]', '2026-11-15T11:00');
    expect(optionTexts('select[name="bookingWasher"]').join(' ')).toContain('Wes');
    await click('.modal-close');

    await chooseSection('Washes');
    await click('.add-record-button');
    await selectValue('select[name="washBooking"]', '#1');
    await submit('Create record');

    await chooseSection('Customers');
    await click('.row-action');
    await selectValue('select[name="customerUser"]', 'sam');
    await submit('Save changes');

    await chooseSection('Washers');
    await click('.row-action');
    await setChecked('input[name="washerAvailable"]', false);
    await submit('Save changes');

    await chooseSection('Vehicles');
    await click('.row-action');
    await setValue('input[name="vehicleColour"]', 'Red');
    await submit('Save changes');

    await chooseSection('Services');
    await click('.row-action');
    await setValue('input[name="servicePrice"]', '125');
    await submit('Save changes');

    await chooseSection('Bookings');
    await click('.row-action');
    await selectValue('select[name="bookingStatus"]', 'Confirmed');
    await submit('Save changes');

    await chooseSection('Washes');
    await click('.row-action');
    await setChecked('input[name="washPaid"]', true);
    await setValue('input[name="washPrice"]', '125');
    await submit('Save changes');

    expect(calls.map((call) => [call.section, call.id])).toEqual([
      ['customers', null],
      ['washers', null],
      ['washers', null],
      ['vehicles', null],
      ['services', null],
      ['bookings', null],
      ['washes', null],
      ['customers', 1],
      ['washers', 1],
      ['vehicles', 1],
      ['services', 1],
      ['bookings', 1],
      ['washes', 1],
    ]);
    expect(data.customers[0].userId).toBe(12);
    expect(data.washers[0].isAvailable).toBe(false);
    expect(data.vehicles[0].colour).toBe('Red');
    expect(data.services[0].price).toBe(125);
    expect(data.bookings[0].status).toBe('Confirmed');
    expect(data.washes[0].isPaid).toBe(true);
    expect(data.washes[0].price).toBe(125);
  });

  function applyPayload(section: ManagementSection, id: number, payload: ManagementPayload): void {
    switch (section) {
      case 'customers': {
        const values = payload as Pick<CustomerRecord, 'userId'>;
        const user = data.users.find((item) => item.id === values.userId);
        const record: CustomerRecord = {
          id,
          userId: values.userId,
          firstName: user?.firstName ?? '',
          lastName: user?.lastName ?? '',
        };
        data.customers = replaceRecord(data.customers, record);
        break;
      }
      case 'washers': {
        const values = payload as Pick<WasherRecord, 'userId' | 'isAvailable' | 'hiredOn'>;
        const user = data.users.find((item) => item.id === values.userId);
        const record: WasherRecord = {
          id,
          ...values,
          firstName: user?.firstName ?? '',
          lastName: user?.lastName ?? '',
        };
        data.washers = replaceRecord(data.washers, record);
        break;
      }
      case 'bookings':
        data.bookings = replaceRecord(data.bookings, { id, ...(payload as Omit<BookingRecord, 'id'>) });
        break;
      case 'vehicles':
        data.vehicles = replaceRecord(data.vehicles, { id, ...(payload as Omit<VehicleRecord, 'id'>) });
        break;
      case 'services':
        data.services = replaceRecord(data.services, { id, ...(payload as Omit<ServiceRecord, 'id'>) });
        break;
      case 'washes':
        data.washes = replaceRecord(data.washes, { id, ...(payload as Omit<WashRecord, 'id'>) });
        break;
    }
  }

  function replaceRecord<T extends { id: number }>(records: T[], updated: T): T[] {
    const index = records.findIndex((record) => record.id === updated.id);
    if (index < 0) {
      return [...records, updated];
    }
    return records.map((record) => (record.id === updated.id ? updated : record));
  }

  async function chooseSection(name: string): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    const button = Array.from(root.querySelectorAll<HTMLButtonElement>('.dashboard-nav button'))
      .find((item) => item.textContent?.includes(name));
    button?.click();
    await settle();
  }

  async function click(selector: string): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    root.querySelector<HTMLButtonElement>(selector)?.click();
    await settle();
  }

  async function selectValue(selector: string, text: string): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    const select = root.querySelector<HTMLSelectElement>(selector);
    const option = Array.from(select?.options ?? []).find((item) => item.textContent?.includes(text));
    if (!select || !option) {
      throw new Error(`Could not find option "${text}" for ${selector}.`);
    }
    select.selectedIndex = option.index;
    select.dispatchEvent(new Event('change', { bubbles: true }));
    await settle();
  }

  function optionTexts(selector: string): string[] {
    const root = fixture.nativeElement as HTMLElement;
    const select = root.querySelector<HTMLSelectElement>(selector);
    return Array.from(select?.options ?? []).map((option) => option.textContent ?? '');
  }

  function bookingWasherDisabled(): boolean {
    const root = fixture.nativeElement as HTMLElement;
    return root.querySelector<HTMLSelectElement>('select[name="bookingWasher"]')?.disabled ?? true;
  }

  async function setValue(selector: string, value: string): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    const input = root.querySelector<HTMLInputElement>(selector);
    if (!input) {
      throw new Error(`Could not find input ${selector}.`);
    }
    input.value = value;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await settle();
  }

  async function setChecked(selector: string, checked: boolean): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    const input = root.querySelector<HTMLInputElement>(selector);
    if (!input) {
      throw new Error(`Could not find checkbox ${selector}.`);
    }
    input.checked = checked;
    input.dispatchEvent(new Event('change', { bubbles: true }));
    await settle();
  }

  async function submit(name: string): Promise<void> {
    const root = fixture.nativeElement as HTMLElement;
    const button = Array.from(root.querySelectorAll<HTMLButtonElement>('.record-form-actions button'))
      .find((item) => item.textContent?.includes(name));
    if (!button || button.disabled) {
      throw new Error(`The "${name}" form action is unavailable.`);
    }
    button.click();
    await settle();
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

});
