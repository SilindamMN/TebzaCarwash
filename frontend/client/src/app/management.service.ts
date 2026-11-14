import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export interface CustomerRecord {
  id: number;
  userId: number;
  firstName: string;
  lastName: string;
}

export interface WasherRecord {
  id: number;
  userId: number;
  isAvailable: boolean;
  hiredOn: string;
  firstName: string;
  lastName: string;
}

export interface BookingRecord {
  id: number;
  customerId: number;
  vehicleId: number;
  serviceId: number;
  washerId: number | null;
  scheduledAt: string;
  status: string;
}

export interface VehicleRecord {
  id: number;
  customerId: number;
  registration: string;
  make: string | null;
  model: string | null;
  colour: string | null;
  type: string;
}

export interface ServiceRecord {
  id: number;
  name: string;
  description: string | null;
  price: number;
  durationMinutes: number;
}

export interface WashRecord {
  id: number;
  bookingId: number | null;
  customerId: number;
  vehicleId: number;
  serviceId: number;
  washerId: number;
  price: number;
  isPaid: boolean;
  status: string;
  startedAt: string;
  completedAt: string | null;
}

export interface UserRecord {
  id: number;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  roles: string[];
}

export interface ManagementData {
  users: UserRecord[];
  customers: CustomerRecord[];
  washers: WasherRecord[];
  bookings: BookingRecord[];
  vehicles: VehicleRecord[];
  services: ServiceRecord[];
  washes: WashRecord[];
}

export type ManagementSection = keyof Omit<ManagementData, 'users'>;

export type ManagementPayload =
  | Pick<CustomerRecord, 'userId'>
  | Pick<WasherRecord, 'userId' | 'isAvailable' | 'hiredOn'>
  | Omit<BookingRecord, 'id'>
  | Omit<VehicleRecord, 'id'>
  | Omit<ServiceRecord, 'id'>
  | Omit<WashRecord, 'id'>;

@Injectable({ providedIn: 'root' })
export class ManagementService {
  private readonly apiUrl = 'http://localhost:5163/api';

  constructor(private readonly http: HttpClient) {}

  loadAll(accessToken: string | null): Promise<ManagementData> {
    const options = this.requestOptions(accessToken);

    return Promise.all([
      firstValueFrom(this.http.get<UserRecord[]>(`${this.apiUrl}/auth/users`, options)),
      firstValueFrom(this.http.get<CustomerRecord[]>(`${this.apiUrl}/customers`, options)),
      firstValueFrom(this.http.get<WasherRecord[]>(`${this.apiUrl}/washers`, options)),
      firstValueFrom(this.http.get<BookingRecord[]>(`${this.apiUrl}/bookings`, options)),
      firstValueFrom(this.http.get<VehicleRecord[]>(`${this.apiUrl}/vehicles`, options)),
      firstValueFrom(this.http.get<ServiceRecord[]>(`${this.apiUrl}/services`, options)),
      firstValueFrom(this.http.get<WashRecord[]>(`${this.apiUrl}/washes`, options)),
    ]).then(([users, customers, washers, bookings, vehicles, services, washes]) => ({
        users,
        customers,
        washers,
        bookings,
        vehicles,
        services,
        washes,
    }));
  }

  async saveRecord(
    accessToken: string | null,
    section: ManagementSection,
    id: number | null,
    payload: ManagementPayload,
  ): Promise<void> {
    const options = this.requestOptions(accessToken);
    const url = `${this.apiUrl}/${section}`;

    if (id === null) {
      await firstValueFrom(this.http.post<unknown>(url, payload, options));
      return;
    }

    await firstValueFrom(this.http.put<unknown>(`${url}/${id}`, { id, ...payload }, options));
  }

  private requestOptions(accessToken: string | null): { headers: HttpHeaders } {
    if (!accessToken) {
      throw new Error('You must sign in before managing records.');
    }

    return {
      headers: new HttpHeaders({ Authorization: `Bearer ${accessToken}` }),
    };
  }
}
