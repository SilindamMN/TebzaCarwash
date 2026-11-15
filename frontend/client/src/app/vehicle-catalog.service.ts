import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface VpicResponse<T> {
  Results: T[];
}

interface VpicMake {
  Make_Name: string | null;
}

interface VpicModel {
  Model_Name: string | null;
}

@Injectable({ providedIn: 'root' })
export class VehicleCatalogService {
  private readonly apiUrl = 'https://vpic.nhtsa.dot.gov/api/vehicles';

  constructor(private readonly http: HttpClient) {}

  async getMakes(): Promise<string[]> {
    const response = await firstValueFrom(
      this.http.get<VpicResponse<VpicMake>>(
        `${this.apiUrl}/GetMakesForVehicleType/car?format=json`,
      ),
    );

    const makes = [...new Set(response.Results.map((item) => item.Make_Name?.trim() ?? '').filter(Boolean))]
      .sort((left, right) => left.localeCompare(right));
    if (!makes.length) {
      throw new Error('The vehicle catalog returned no makes.');
    }
    return makes;
  }

  async getModels(make: string): Promise<string[]> {
    const response = await firstValueFrom(
      this.http.get<VpicResponse<VpicModel>>(
        `${this.apiUrl}/GetModelsForMake/${encodeURIComponent(make)}?format=json`,
      ),
    );

    const models = [...new Set(response.Results.map((item) => item.Model_Name?.trim() ?? '').filter(Boolean))]
      .sort((left, right) => left.localeCompare(right));
    if (!models.length) {
      throw new Error(`The vehicle catalog returned no models for ${make}.`);
    }
    return models;
  }
}
