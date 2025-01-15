import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControlOptions, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AccountService } from '../../core/services/account.service';

@Component({
  selector: 'app-register',
  imports: [
    NgIf,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButton,
    MatIconButton,
    MatIcon
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  form?: FormGroup;
  isLoading = false;
  showPassword = false;
  showComfirmPassword = false;
  returnUrl?: string;
  constructor(private route: ActivatedRoute, private router: Router, private fb: FormBuilder, private accountService: AccountService,) {
    if (this.route.snapshot.queryParamMap.has('returnUrl')) {
      this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || undefined;
    }
    this.form = this.fb.group(
      {
        email: this.fb.control('', [Validators.required, Validators.email]),
        password: this.fb.control('', [Validators.required, Validators.minLength(8)]),
        confirmPassword: this.fb.control('', [Validators.required]),
      }, {
        validators: this.passwordMatchValidator
      } as AbstractControlOptions);
  }

  passwordMatchValidator(form: FormGroup) {
    return form.get('password')?.value === form.get('confirmPassword')?.value ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.isLoading || this.form?.invalid) return;
    this.isLoading = true;
    this.accountService.register(this.form?.value)
      .subscribe({
        next: (user) => {
          this.router.navigate(['/confirm-email'], { queryParams: { id: user.id, returnUrl: this.returnUrl } });
        },
        error: (err) => {
          console.error(err);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }
}
