import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ManagementPayload, ManagementSection, ManagementService } from './management.service';

describe('ManagementService', () => {
  let service: ManagementService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ManagementService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ManagementService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('creates a record through its section endpoint with the access token', async () => {
    const saved = service.saveRecord('token', 'services', null, {
      name: 'Premium wash',
      description: null,
      price: 250,
      durationMinutes: 45,
    });

    const request = http.expectOne('http://localhost:5163/api/services');
    expect(request.request.method).toBe('POST');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token');
    expect(request.request.body).toEqual({
      name: 'Premium wash',
      description: null,
      price: 250,
      durationMinutes: 45,
    });
    request.flush({ isSucceed: true });
    await saved;
  });

  it('updates the selected record with its ID in the route and body', async () => {
    const saved = service.saveRecord('token', 'vehicles', 12, {
      customerId: 3,
      registration: 'XYZ789',
      make: 'Toyota',
      model: 'Corolla',
      colour: 'Blue',
      type: 'Sedan',
    });

    const request = http.expectOne('http://localhost:5163/api/vehicles/12');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      id: 12,
      customerId: 3,
      registration: 'XYZ789',
      make: 'Toyota',
      model: 'Corolla',
      colour: 'Blue',
      type: 'Sedan',
    });
    request.flush({ isSucceed: true });
    await saved;
  });

  it('uses the correct create and update routes for every record type', async () => {
    const records: Array<{ section: ManagementSection; payload: ManagementPayload }> = [
      { section: 'customers', payload: { userId: 10 } },
      { section: 'washers', payload: { userId: 11, isAvailable: true, hiredOn: '2026-01-01T00:00:00.000Z' } },
      {
        section: 'bookings',
        payload: {
          customerId: 1,
          vehicleId: 1,
          serviceId: 1,
          washerId: 1,
          scheduledAt: '2026-01-02T10:00:00.000Z',
          status: 'Requested',
        },
      },
      {
        section: 'vehicles',
        payload: { customerId: 1, registration: 'ABC123', make: 'Toyota', model: 'Corolla', colour: 'Blue', type: 'Sedan' },
      },
      { section: 'services', payload: { name: 'Wash', description: null, price: 100, durationMinutes: 30 } },
      {
        section: 'washes',
        payload: {
          bookingId: 1,
          customerId: 1,
          vehicleId: 1,
          serviceId: 1,
          washerId: 1,
          price: 100,
          isPaid: false,
          status: 'Pending',
          startedAt: '2026-01-02T10:00:00.000Z',
          completedAt: null,
        },
      },
    ];

    for (const { section, payload } of records) {
      const create = service.saveRecord('token', section, null, payload);
      const createRequest = http.expectOne(`http://localhost:5163/api/${section}`);
      expect(createRequest.request.method).toBe('POST');
      expect(createRequest.request.headers.get('Authorization')).toBe('Bearer token');
      expect(createRequest.request.body).toEqual(payload);
      createRequest.flush({ isSucceed: true });
      await create;

      const update = service.saveRecord('token', section, 7, payload);
      const updateRequest = http.expectOne(`http://localhost:5163/api/${section}/7`);
      expect(updateRequest.request.method).toBe('PUT');
      expect(updateRequest.request.body).toEqual({ id: 7, ...payload });
      updateRequest.flush({ isSucceed: true });
      await update;
    }
  });

  it('loads user accounts and all six management directories', async () => {
    const loaded = service.loadAll('token');
    const user = {
      id: 10,
      firstName: 'Ava',
      lastName: 'Customer',
      userName: 'ava',
      email: 'ava@example.com',
      roles: ['USER'],
    };
    const endpoints = [
      ['auth/users', [user]],
      ['customers', []],
      ['washers', []],
      ['bookings', []],
      ['vehicles', []],
      ['services', []],
      ['washes', []],
    ] as const;

    for (const [endpoint, response] of endpoints) {
      const request = http.expectOne(`http://localhost:5163/api/${endpoint}`);
      expect(request.request.method).toBe('GET');
      expect(request.request.headers.get('Authorization')).toBe('Bearer token');
      request.flush(response);
    }

    expect(await loaded).toEqual({
      users: [user],
      customers: [],
      washers: [],
      bookings: [],
      vehicles: [],
      services: [],
      washes: [],
    });
  });
});
