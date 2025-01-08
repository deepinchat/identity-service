import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators, AbstractControlOptions } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';

@Component({
  selector: 'app-reset-password',
  imports: [
    NgIf,
    RouterLink,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButton,
    MatIcon,
    MatIconButton,
    MatCardModule
  ],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent {
  form?: FormGroup;
  isLoading = false;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private accountService: AccountService
  ) { }

  ngOnInit() {
    if (this.route.snapshot.queryParamMap.has('email')) {

      this.form = this.fb.group({
        email: this.fb.control(this.route.snapshot.queryParamMap.get('email'), [Validators.email, Validators.required]),
        code: this.fb.control('', [Validators.required, Validators.minLength(4), Validators.maxLength(6)]),
        password: this.fb.control('', [Validators.required, Validators.minLength(8), Validators.maxLength(32)]),
        confirmPassword: this.fb.control('', [Validators.required])
      }, {
        validators: this.passwordMatchValidator
      } as AbstractControlOptions);
    }
  }

  passwordMatchValidator(form: FormGroup) {
    return form.get('password')?.value === form.get('confirmPassword')?.value ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.isLoading || this.form?.invalid) return;
    this.isLoading = true;
    this.accountService.resetPassword(this.form?.value)
      .subscribe({
        next: () => {
          this.router.navigate(['../login'], { relativeTo: this.route });
        },
        error: () => {

        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }
}
