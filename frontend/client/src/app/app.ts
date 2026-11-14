import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from './auth.service';
import { Dashboard } from './dashboard';

@Component({
  imports: [Dashboard, FormsModule],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {
  protected isRegistering = false;
  protected isSubmitting = false;
  protected notice = '';
  protected noticeType: 'success' | 'error' = 'success';
  protected firstName = '';
  protected lastName = '';
  protected userName = '';
  protected email = '';
  protected password = '';
  protected confirmPassword = '';

  constructor(protected readonly auth: AuthService) {}

  protected toggleMode(): void {
    this.isRegistering = !this.isRegistering;
    this.notice = '';
    this.password = '';
    this.confirmPassword = '';
  }

  protected async submit(): Promise<void> {
    if (this.isSubmitting || (this.isRegistering && this.password !== this.confirmPassword)) {
      return;
    }

    this.isSubmitting = true;
    this.notice = '';

    try {
      if (this.isRegistering) {
        await this.auth.register({
          firstName: this.firstName.trim(),
          lastName: this.lastName.trim(),
          userName: this.userName.trim(),
          email: this.email.trim(),
          password: this.password,
        });
        this.isRegistering = false;
        this.password = '';
        this.confirmPassword = '';
        this.noticeType = 'success';
        this.notice = 'Your account has been created. Sign in to continue.';
      } else {
        await this.auth.login(this.userName.trim(), this.password);
      }
    } catch (error: unknown) {
      this.noticeType = 'error';
      this.notice = this.auth.getErrorMessage(error);
    } finally {
      this.isSubmitting = false;
    }
  }

}
