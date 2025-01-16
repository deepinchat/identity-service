import { Component, inject } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { NgIf } from '@angular/common';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-register-confirmation',
  imports: [
    NgIf,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButton,
    MatIconButton,
    MatCardModule,
    MatIcon
  ],
  templateUrl: './register-confirmation.component.html',
  styleUrl: './register-confirmation.component.scss'
})
export class RegisterConfirmationComponent {
  form?: FormGroup;
  isLoading = false;
  isSending = false;
  sendInterval = 0;
  private snackBar = inject(MatSnackBar);
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private accountService: AccountService
  ) {
  }

  ngOnInit() {
    if (this.route.snapshot.queryParamMap.has('id')) {
      this.form = this.fb.group({
        userId: this.fb.control(this.route.snapshot.queryParamMap.get('id')),
        code: this.fb.control('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]),
      });
    }
  }

  resendEmailConfirmation() {
    if (this.isSending || this.sendInterval > 0) {
      return;
    }
    this.isSending = true;
    this.accountService.resendEmailConfirmation(this.form?.value.userId)
      .subscribe({
        next: () => {
          this.snackBar.open('Email sent', 'Close', {
            duration: 3000
          });
        },
        error: () => {
          this.snackBar.open('Failed to send email', 'Close', {
            duration: 3000
          });
        },
        complete: () => {
          this.isSending = false;
          this.sendInterval = 60;
          const interval = setInterval(() => {
            if (this.sendInterval <= 0) {
              clearInterval(interval);
              return;
            }
            this.sendInterval--;
          }, 1000);
        }
      });
  }

  onSubmit() {
    if (this.isLoading || this.form?.invalid) return;
    this.isLoading = true;
    this.accountService.confirmEmail(this.form?.value)
      .subscribe({
        next: () => {
          this.router.navigate(['../signin'], { relativeTo: this.route });
        },
        error: () => {

        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }
}
