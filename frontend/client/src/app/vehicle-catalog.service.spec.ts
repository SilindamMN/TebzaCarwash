import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { VehicleCatalogService } from './vehicle-catalog.service';

describe('VehicleCatalogService', () => {
  let service: VehicleCatalogService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [VehicleCatalogService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VehicleCatalogService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads unique, sorted makes from the vehicle catalog', async () => {
    const makes = service.getMakes();
    const request = http.expectOne(
      'https://vpic.nhtsa.dot.gov/api/vehicles/GetMakesForVehicleType/car?format=json',
    );
    request.flush({ Results: [{ Make_Name: 'Toyota' }, { Make_Name: 'Ford' }, { Make_Name: 'Toyota' }] });
    expect(await makes).toEqual(['Ford', 'Toyota']);
  });

  it('loads models for the selected make', async () => {
    const models = service.getModels('Aston Martin');
    const request = http.expectOne(
      'https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMake/Aston%20Martin?format=json',
    );
    request.flush({ Results: [{ Model_Name: 'Vantage' }, { Model_Name: 'DBX' }] });
    expect(await models).toEqual(['DBX', 'Vantage']);
  });
});
