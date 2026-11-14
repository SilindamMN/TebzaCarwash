import { environment } from '../environments/environment';

export const apiNotConfiguredMessage = 'The backend API is not configured for this deployment.';

export function requireApiBaseUrl(): string {
  if (!environment.apiBaseUrl) {
    throw new Error(apiNotConfiguredMessage);
  }

  return environment.apiBaseUrl;
}
