import { Component, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './auth.service';
import {
  BookingRecord,
  CustomerRecord,
  ManagementData,
  ManagementService,
  ServiceRecord,
  VehicleRecord,
  WasherRecord,
  WashRecord,
} from './management.service';

type DashboardSection =
  | 'overview'
  | 'customers'
  | 'washers'
  | 'bookings'
  | 'vehicles'
  | 'services'
  | 'washes';

@Component({
  imports: [CurrencyPipe, DatePipe, FormsModule, UpperCasePipe],
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
})
export class Dashboard {
  protected readonly data = signal<ManagementData | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly error = signal('');
  protected activeSection: DashboardSection = 'overview';
  protected searchTerm = '';
  protected menuOpen = false;

  protected readonly navItems: ReadonlyArray<{
    id: DashboardSection;
    label: string;
    icon: string;
  }> = [
    { id: 'overview', label: 'Overview', icon: '◫' },
    { id: 'customers', label: 'Customers', icon: '○' },
    { id: 'washers', label: 'Washers', icon: '◇' },
    { id: 'bookings', label: 'Bookings', icon: '▦' },
    { id: 'vehicles', label: 'Vehicles', icon: '⌁' },
    { id: 'services', label: 'Services', icon: '✳' },
    { id: 'washes', label: 'Washes', icon: '≋' },
  ];

  constructor(
    protected readonly auth: AuthService,
    private readonly management: ManagementService,
  ) {
    void this.refresh();
  }

  protected get sectionTitle(): string {
    return this.navItems.find((item) => item.id === this.activeSection)?.label ?? 'Overview';
  }

  protected get customers(): CustomerRecord[] {
    return this.filterRecords(this.data()?.customers, (customer) =>
      `${customer.id} ${customer.userId} ${customer.firstName} ${customer.lastName}`,
    );
  }

  protected get washers(): WasherRecord[] {
    return this.filterRecords(this.data()?.washers, (washer) =>
      `${washer.id} ${washer.userId} ${washer.firstName} ${washer.lastName} ${washer.isAvailable ? 'available' : 'busy'}`,
    );
  }

  protected get bookings(): BookingRecord[] {
    return this.filterRecords(this.data()?.bookings, (booking) =>
      `${booking.id} ${this.customerName(booking.customerId)} ${this.vehicleName(booking.vehicleId)} ${this.serviceName(booking.serviceId)} ${booking.status}`,
    );
  }

  protected get vehicles(): VehicleRecord[] {
    return this.filterRecords(this.data()?.vehicles, (vehicle) =>
      `${vehicle.id} ${vehicle.registration} ${vehicle.make ?? ''} ${vehicle.model ?? ''} ${this.customerName(vehicle.customerId)}`,
    );
  }

  protected get services(): ServiceRecord[] {
    return this.filterRecords(this.data()?.services, (service) =>
      `${service.name} ${service.description ?? ''}`,
    );
  }

  protected get washes(): WashRecord[] {
    return this.filterRecords(this.data()?.washes, (wash) =>
      `${wash.id} ${this.customerName(wash.customerId)} ${this.vehicleName(wash.vehicleId)} ${this.serviceName(wash.serviceId)} ${wash.status} ${wash.isPaid ? 'paid' : 'unpaid'}`,
    );
  }

  protected get activeBookings(): number {
    return (this.data()?.bookings ?? []).filter((booking) =>
      ['requested', 'confirmed'].includes(booking.status.toLowerCase()),
    ).length;
  }

  protected get activeWashes(): number {
    return (this.data()?.washes ?? []).filter((wash) =>
      ['pending', 'inprogress', 'in progress'].includes(wash.status.toLowerCase()),
    ).length;
  }

  protected get collectedRevenue(): number {
    return (this.data()?.washes ?? [])
      .filter((wash) => wash.isPaid)
      .reduce((total, wash) => total + wash.price, 0);
  }

  protected get upcomingBookings(): BookingRecord[] {
    return [...(this.data()?.bookings ?? [])]
      .filter((booking) => Date.parse(booking.scheduledAt) >= Date.now())
      .filter((booking) => booking.status.toLowerCase() !== 'cancelled')
      .sort((left, right) => Date.parse(left.scheduledAt) - Date.parse(right.scheduledAt))
      .slice(0, 5);
  }

  protected get recentWashes(): WashRecord[] {
    return [...(this.data()?.washes ?? [])]
      .sort((left, right) => Date.parse(right.startedAt) - Date.parse(left.startedAt))
      .slice(0, 4);
  }

  protected countFor(section: DashboardSection): number {
    const data = this.data();
    if (!data) {
      return 0;
    }

    switch (section) {
      case 'customers':
        return data.customers.length;
      case 'washers':
        return data.washers.length;
      case 'bookings':
        return data.bookings.length;
      case 'vehicles':
        return data.vehicles.length;
      case 'services':
        return data.services.length;
      case 'washes':
        return data.washes.length;
      default:
        return 0;
    }
  }

  protected selectSection(section: DashboardSection): void {
    this.activeSection = section;
    this.searchTerm = '';
    this.menuOpen = false;
  }

  protected customerName(id: number): string {
    const customer = this.data()?.customers.find((record) => record.id === id);
    if (!customer) {
      return `Customer #${id}`;
    }
    return `${customer.firstName} ${customer.lastName}`.trim() || `Customer #${id}`;
  }

  protected washerName(id: number | null): string {
    if (id === null) {
      return 'Not assigned';
    }
    const washer = this.data()?.washers.find((record) => record.id === id);
    return washer
      ? `${washer.firstName} ${washer.lastName}`.trim() || `Washer #${id}`
      : `Washer #${id}`;
  }

  protected vehicleName(id: number): string {
    const vehicle = this.data()?.vehicles.find((record) => record.id === id);
    if (!vehicle) {
      return `Vehicle #${id}`;
    }
    const description = [vehicle.make, vehicle.model].filter(Boolean).join(' ');
    return description ? `${description} · ${vehicle.registration}` : vehicle.registration;
  }

  protected serviceName(id: number): string {
    return this.data()?.services.find((service) => service.id === id)?.name ?? `Service #${id}`;
  }

  protected vehicleCountFor(customerId: number): number {
    return (this.data()?.vehicles ?? []).filter((vehicle) => vehicle.customerId === customerId).length;
  }

  protected bookingCountFor(customerId: number): number {
    return (this.data()?.bookings ?? []).filter((booking) => booking.customerId === customerId).length;
  }

  protected customerWashCountFor(customerId: number): number {
    return (this.data()?.washes ?? []).filter((wash) => wash.customerId === customerId).length;
  }

  protected washCountFor(washerId: number): number {
    return (this.data()?.washes ?? []).filter((wash) => wash.washerId === washerId).length;
  }

  protected serviceBookingCount(serviceId: number): number {
    return (this.data()?.bookings ?? []).filter((booking) => booking.serviceId === serviceId).length;
  }

  protected isAvailable(washer: WasherRecord): boolean {
    return washer.isAvailable;
  }

  protected isCompleted(wash: WashRecord): boolean {
    return wash.status.toLowerCase() === 'completed';
  }

  protected statusClass(status: string): string {
    return status.toLowerCase().replace(/[^a-z]+/g, '-');
  }

  protected signOut(): void {
    this.auth.signOut();
  }

  protected async refresh(): Promise<void> {
    this.isLoading.set(true);
    this.error.set('');

    try {
      this.data.set(await this.management.loadAll(this.auth.accessToken));
    } catch (error: unknown) {
      this.error.set(this.auth.getErrorMessage(error));
    } finally {
      this.isLoading.set(false);
    }
  }

  private filterRecords<T>(records: T[] | undefined, getSearchText: (record: T) => string): T[] {
    const query = this.searchTerm.trim().toLowerCase();
    return (records ?? []).filter((record) => getSearchText(record).toLowerCase().includes(query));
  }
}
