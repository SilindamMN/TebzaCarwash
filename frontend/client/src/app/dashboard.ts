import { Component, DestroyRef, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { AuthService } from './auth.service';
import {
  BookingRecord,
  CustomerRecord,
  ManagementData,
  ManagementPayload,
  ManagementSection,
  ManagementService,
  ServiceRecord,
  UserRecord,
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

interface CustomerForm {
  userId: number | null;
}

interface WasherForm {
  userId: number | null;
  isAvailable: boolean;
  hiredOn: string;
}

interface BookingForm {
  customerId: number | null;
  vehicleId: number | null;
  serviceId: number | null;
  washerId: number | null;
  scheduledAt: string;
  status: string;
}

interface VehicleForm {
  customerId: number | null;
  registration: string;
  make: string;
  model: string;
  colour: string;
  type: string;
}

interface ServiceForm {
  name: string;
  description: string;
  price: number;
  durationMinutes: number;
}

interface WashForm {
  bookingId: number | null;
  customerId: number | null;
  vehicleId: number | null;
  serviceId: number | null;
  washerId: number | null;
  price: number;
  isPaid: boolean;
  status: string;
  startedAt: string;
  completedAt: string;
}

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
  protected readonly recordFormOpen = signal(false);
  protected readonly isSavingRecord = signal(false);
  protected readonly recordFormError = signal('');
  protected editingRecordId: number | null = null;
  protected customerForm: CustomerForm = { userId: null };
  protected washerForm: WasherForm = { userId: null, isAvailable: true, hiredOn: this.today() };
  protected bookingForm: BookingForm = {
    customerId: null,
    vehicleId: null,
    serviceId: null,
    washerId: null,
    scheduledAt: '',
    status: 'Requested',
  };
  protected vehicleForm: VehicleForm = {
    customerId: null,
    registration: '',
    make: '',
    model: '',
    colour: '',
    type: 'Sedan',
  };
  protected serviceForm: ServiceForm = { name: '', description: '', price: 0, durationMinutes: 30 };
  protected washForm: WashForm = {
    bookingId: null,
    customerId: null,
    vehicleId: null,
    serviceId: null,
    washerId: null,
    price: 0,
    isPaid: false,
    status: 'Pending',
    startedAt: this.localDateTimeNow(),
    completedAt: '',
  };
  protected readonly vehicleTypes = ['Hatchback', 'Sedan', 'SUV', 'Bakkie', 'Van', 'Motorcycle'];
  protected readonly bookingStatuses = ['Requested', 'Confirmed', 'Completed', 'Cancelled'];
  protected readonly washStatuses = ['Pending', 'InProgress', 'Completed', 'Cancelled'];
  protected readonly openingTime = '09:00';
  protected readonly closingTime = '21:00';

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
    destroyRef: DestroyRef,
  ) {
    void this.refresh();
    interval(60_000)
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe(() => {
        if (this.data() && !this.isLoading()) {
          void this.refresh();
        }
      });
  }

  protected get sectionTitle(): string {
    return this.navItems.find((item) => item.id === this.activeSection)?.label ?? 'Overview';
  }

  protected get sectionRecordTitle(): string {
    switch (this.activeSection) {
      case 'customers':
        return 'Customer';
      case 'washers':
        return 'Washer';
      case 'bookings':
        return 'Booking';
      case 'vehicles':
        return 'Vehicle';
      case 'services':
        return 'Service';
      case 'washes':
        return 'Wash';
      default:
        return 'Record';
    }
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

  protected openCreateRecord(): void {
    if (this.activeSection === 'overview') {
      return;
    }

    this.editingRecordId = null;
    this.recordFormError.set('');
    switch (this.activeSection) {
      case 'customers':
        this.customerForm = { userId: null };
        break;
      case 'washers':
        this.washerForm = { userId: null, isAvailable: true, hiredOn: this.today() };
        break;
      case 'bookings':
        this.bookingForm = {
          customerId: null,
          vehicleId: null,
          serviceId: null,
          washerId: null,
          scheduledAt: '',
          status: 'Requested',
        };
        break;
      case 'vehicles':
        this.vehicleForm = {
          customerId: null,
          registration: '',
          make: '',
          model: '',
          colour: '',
          type: 'Sedan',
        };
        break;
      case 'services':
        this.serviceForm = { name: '', description: '', price: 0, durationMinutes: 30 };
        break;
      case 'washes':
        this.washForm = {
          bookingId: null,
          customerId: null,
          vehicleId: null,
          serviceId: null,
          washerId: null,
          price: 0,
          isPaid: false,
          status: 'Pending',
          startedAt: this.localDateTimeNow(),
          completedAt: '',
        };
        break;
    }
    this.recordFormOpen.set(true);
  }

  protected editCustomer(record: CustomerRecord): void {
    this.beginEdit(record.id);
    this.customerForm = { userId: record.userId };
  }

  protected editWasher(record: WasherRecord): void {
    this.beginEdit(record.id);
    this.washerForm = {
      userId: record.userId,
      isAvailable: record.isAvailable,
      hiredOn: this.localDateInput(record.hiredOn),
    };
  }

  protected editBooking(record: BookingRecord): void {
    this.beginEdit(record.id);
    this.bookingForm = {
      customerId: record.customerId,
      vehicleId: record.vehicleId,
      serviceId: record.serviceId,
      washerId: record.washerId,
      scheduledAt: this.localDateTimeInput(record.scheduledAt),
      status: record.status,
    };
  }

  protected editVehicle(record: VehicleRecord): void {
    this.beginEdit(record.id);
    this.vehicleForm = {
      customerId: record.customerId,
      registration: record.registration,
      make: record.make ?? '',
      model: record.model ?? '',
      colour: record.colour ?? '',
      type: record.type,
    };
  }

  protected editService(record: ServiceRecord): void {
    this.beginEdit(record.id);
    this.serviceForm = {
      name: record.name,
      description: record.description ?? '',
      price: record.price,
      durationMinutes: record.durationMinutes,
    };
  }

  protected editWash(record: WashRecord): void {
    this.beginEdit(record.id);
    this.washForm = {
      bookingId: record.bookingId,
      customerId: record.customerId,
      vehicleId: record.vehicleId,
      serviceId: record.serviceId,
      washerId: record.washerId,
      price: record.price,
      isPaid: record.isPaid,
      status: record.status,
      startedAt: this.localDateTimeInput(record.startedAt),
      completedAt: record.completedAt ? this.localDateTimeInput(record.completedAt) : '',
    };
  }

  protected closeRecordForm(): void {
    if (!this.isSavingRecord()) {
      this.recordFormOpen.set(false);
    }
  }

  protected availableUsers(section: 'customers' | 'washers'): UserRecord[] {
    const records = section === 'customers' ? this.data()?.customers ?? [] : this.data()?.washers ?? [];
    const oppositeRecords = section === 'customers' ? this.data()?.washers ?? [] : this.data()?.customers ?? [];
    const oppositeRole = section === 'customers' ? 'WASHER' : 'CUSTOMER';
    return (this.data()?.users ?? []).filter((user) =>
      !records.some((record) => record.userId === user.id && record.id !== this.editingRecordId) &&
      !oppositeRecords.some((record) => record.userId === user.id) &&
      !this.hasRole(user, oppositeRole),
    );
  }

  protected customersForAssignment(): CustomerRecord[] {
    const washerUserIds = new Set((this.data()?.washers ?? []).map((washer) => washer.userId));
    return (this.data()?.customers ?? []).filter(
      (customer) => !washerUserIds.has(customer.userId) && !this.hasRoleForUser(customer.userId, 'WASHER'),
    );
  }

  protected vehiclesForCustomer(customerId: number | null): VehicleRecord[] {
    return (this.data()?.vehicles ?? []).filter((vehicle) => vehicle.customerId === customerId);
  }

  protected onBookingCustomerChange(customerId: number | null): void {
    const vehicles = this.vehiclesForCustomer(customerId);
    if (!vehicles.some((vehicle) => vehicle.id === this.bookingForm.vehicleId)) {
      this.bookingForm.vehicleId = vehicles[0]?.id ?? null;
    }
  }

  protected onBookingServiceChange(serviceId: number | null): void {
    this.bookingForm.serviceId = serviceId;
    this.clearUnavailableBookingWasher();
  }

  protected onBookingScheduleChange(): void {
    this.clearUnavailableBookingWasher();
  }

  protected get bookingMinDateTime(): string {
    const date = this.bookingForm.scheduledAt.slice(0, 10);
    return date ? `${date}T${this.openingTime}` : '';
  }

  protected get bookingMaxDateTime(): string {
    const date = this.bookingForm.scheduledAt.slice(0, 10);
    if (!date) {
      return '';
    }

    const duration = this.data()?.services.find((service) => service.id === this.bookingForm.serviceId)
      ?.durationMinutes ?? 0;
    const latestStartMinutes = 21 * 60 - duration;
    if (latestStartMinutes < 9 * 60) {
      return `${date}T08:59`;
    }

    const hours = Math.floor(latestStartMinutes / 60).toString().padStart(2, '0');
    const minutes = (latestStartMinutes % 60).toString().padStart(2, '0');
    return `${date}T${hours}:${minutes}`;
  }

  protected get canSelectBookingWasher(): boolean {
    if (!this.bookingForm.scheduledAt || this.bookingForm.serviceId === null) {
      return false;
    }

    const startsAt = Date.parse(this.bookingForm.scheduledAt);
    const service = this.data()?.services.find((item) => item.id === this.bookingForm.serviceId);
    return Number.isFinite(startsAt) &&
      (service?.durationMinutes ?? 0) > 0 &&
      !this.bookingScheduleError;
  }

  protected get bookingScheduleError(): string {
    if (!this.bookingForm.scheduledAt || this.bookingForm.serviceId === null) {
      return '';
    }

    const startsAt = new Date(this.bookingForm.scheduledAt);
    if (Number.isNaN(startsAt.getTime())) {
      return 'Choose a valid booking date and time.';
    }

    const service = this.data()?.services.find((item) => item.id === this.bookingForm.serviceId);
    if (!service) {
      return '';
    }

    const startMinutes = startsAt.getHours() * 60 + startsAt.getMinutes();
    const openingMinutes = 9 * 60;
    const closingMinutes = 21 * 60;
    if (startMinutes < openingMinutes) {
      return `Bookings must start at or after ${this.openingTime}.`;
    }
    if (startMinutes >= closingMinutes || startMinutes + service.durationMinutes > closingMinutes) {
      return `This service must finish by ${this.closingTime}. Choose an earlier time or shorter service.`;
    }
    return '';
  }

  protected availableWashersForBooking(): WasherRecord[] {
    if (!this.canSelectBookingWasher) {
      return [];
    }

    const data = this.data();
    const startsAt = Date.parse(this.bookingForm.scheduledAt);
    const service = data?.services.find((item) => item.id === this.bookingForm.serviceId);
    if (!data || !service) {
      return [];
    }

    const endsAt = startsAt + service.durationMinutes * 60_000;
    return data.washers.filter((washer) =>
      washer.isAvailable &&
      !data.bookings.some((booking) => {
        if (
          booking.washerId !== washer.id ||
          booking.id === (this.activeSection === 'bookings' ? this.editingRecordId : null) ||
          ['cancelled', 'completed'].includes(booking.status.toLowerCase())
        ) {
          return false;
        }

        const bookingStartsAt = Date.parse(booking.scheduledAt);
        const bookedService = data.services.find((item) => item.id === booking.serviceId);
        const bookingDuration = bookedService?.durationMinutes ?? service.durationMinutes;
        const bookingEndsAt = bookingStartsAt + bookingDuration * 60_000;
        return Number.isFinite(bookingStartsAt) && startsAt < bookingEndsAt && bookingStartsAt < endsAt;
      }),
    );
  }

  private clearUnavailableBookingWasher(): void {
    if (
      this.bookingForm.washerId !== null &&
      !this.availableWashersForBooking().some((washer) => washer.id === this.bookingForm.washerId)
    ) {
      this.bookingForm.washerId = null;
    }
  }

  private hasRole(user: UserRecord, role: string): boolean {
    return (user.roles ?? []).some((userRole) => userRole.trim().toUpperCase() === role);
  }

  private hasRoleForUser(userId: number, role: string): boolean {
    const user = this.data()?.users.find((item) => item.id === userId);
    return user ? this.hasRole(user, role) : false;
  }

  protected onWashCustomerChange(customerId: number | null): void {
    const vehicles = this.vehiclesForCustomer(customerId);
    if (!vehicles.some((vehicle) => vehicle.id === this.washForm.vehicleId)) {
      this.washForm.vehicleId = vehicles[0]?.id ?? null;
    }
  }

  protected onWashBookingChange(bookingId: number | null): void {
    this.washForm.bookingId = bookingId;
    const booking = (this.data()?.bookings ?? []).find((item) => item.id === bookingId);
    if (!booking) {
      return;
    }

    this.washForm.customerId = booking.customerId;
    this.washForm.vehicleId = booking.vehicleId;
    this.washForm.serviceId = booking.serviceId;
    this.washForm.washerId = booking.washerId;
    this.washForm.price =
      this.data()?.services.find((service) => service.id === booking.serviceId)?.price ?? this.washForm.price;
  }

  protected onWashServiceChange(serviceId: number | null): void {
    this.washForm.serviceId = serviceId;
    this.washForm.price =
      this.data()?.services.find((service) => service.id === serviceId)?.price ?? this.washForm.price;
  }

  protected async saveRecord(): Promise<void> {
    if (this.activeSection === 'overview' || this.isSavingRecord()) {
      return;
    }

    if (this.activeSection === 'bookings' && this.bookingScheduleError) {
      this.recordFormError.set(this.bookingScheduleError);
      return;
    }

    let payload: ManagementPayload | null;
    try {
      payload = this.createPayload(this.activeSection);
    } catch (error: unknown) {
      this.recordFormError.set(this.auth.getErrorMessage(error));
      return;
    }

    if (!payload) {
      this.recordFormError.set('Select all required linked records before saving.');
      return;
    }

    this.isSavingRecord.set(true);
    this.recordFormError.set('');
    try {
      await this.management.saveRecord(
        this.auth.accessToken,
        this.activeSection,
        this.editingRecordId,
        payload,
      );
      this.recordFormOpen.set(false);
      await this.refresh();
    } catch (error: unknown) {
      this.recordFormError.set(this.auth.getErrorMessage(error));
    } finally {
      this.isSavingRecord.set(false);
    }
  }

  private beginEdit(id: number): void {
    this.editingRecordId = id;
    this.recordFormError.set('');
    this.recordFormOpen.set(true);
  }

  private createPayload(section: ManagementSection): ManagementPayload | null {
    switch (section) {
      case 'customers':
        return this.customerForm.userId === null ? null : { userId: this.customerForm.userId };
      case 'washers':
        return this.washerForm.userId === null
          ? null
          : {
              userId: this.washerForm.userId,
              isAvailable: this.washerForm.isAvailable,
              hiredOn: this.dateToIso(this.washerForm.hiredOn),
            };
      case 'bookings':
        if (
          this.bookingForm.customerId === null ||
          this.bookingForm.vehicleId === null ||
          this.bookingForm.serviceId === null
        ) {
          return null;
        }
        return {
          customerId: this.bookingForm.customerId,
          vehicleId: this.bookingForm.vehicleId,
          serviceId: this.bookingForm.serviceId,
          washerId: this.bookingForm.washerId,
          scheduledAt: this.dateToIso(this.bookingForm.scheduledAt),
          status: this.bookingForm.status,
        };
      case 'vehicles':
        return this.vehicleForm.customerId === null
          ? null
          : {
              customerId: this.vehicleForm.customerId,
              registration: this.vehicleForm.registration.trim(),
              make: this.nullIfBlank(this.vehicleForm.make),
              model: this.nullIfBlank(this.vehicleForm.model),
              colour: this.nullIfBlank(this.vehicleForm.colour),
              type: this.vehicleForm.type,
            };
      case 'services':
        return {
          name: this.serviceForm.name.trim(),
          description: this.nullIfBlank(this.serviceForm.description),
          price: this.serviceForm.price,
          durationMinutes: this.serviceForm.durationMinutes,
        };
      case 'washes':
        if (
          this.washForm.customerId === null ||
          this.washForm.vehicleId === null ||
          this.washForm.serviceId === null ||
          this.washForm.washerId === null
        ) {
          return null;
        }
        return {
          bookingId: this.washForm.bookingId,
          customerId: this.washForm.customerId,
          vehicleId: this.washForm.vehicleId,
          serviceId: this.washForm.serviceId,
          washerId: this.washForm.washerId,
          price: this.washForm.price,
          isPaid: this.washForm.isPaid,
          status: this.washForm.status,
          startedAt: this.dateToIso(this.washForm.startedAt),
          completedAt: this.washForm.completedAt ? this.dateToIso(this.washForm.completedAt) : null,
        };
    }
  }

  private nullIfBlank(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length ? trimmed : null;
  }

  private dateToIso(value: string): string {
    const date = new Date(value);
    if (!value || Number.isNaN(date.getTime())) {
      throw new Error('Enter a valid date and time.');
    }
    return date.toISOString();
  }

  private today(): string {
    return this.localDateInput(new Date().toISOString());
  }

  private localDateInput(value: string): string {
    if (!value) {
      return '';
    }
    return this.localDateTimeInput(value).slice(0, 10);
  }

  private localDateTimeInput(value: string): string {
    if (!value) {
      return '';
    }
    if (!/(Z|[+-]\d{2}:\d{2})$/i.test(value)) {
      return value.includes('T') ? value.slice(0, 16) : `${value}T00:00`;
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }
    date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    return date.toISOString().slice(0, 16);
  }

  private localDateTimeNow(): string {
    return this.localDateTimeInput(new Date().toISOString());
  }

  private filterRecords<T>(records: T[] | undefined, getSearchText: (record: T) => string): T[] {
    const query = this.searchTerm.trim().toLowerCase();
    return (records ?? []).filter((record) => getSearchText(record).toLowerCase().includes(query));
  }
}
