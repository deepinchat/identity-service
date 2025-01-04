import { Component } from '@angular/core';
import { AccountService } from '../../core/services/account.service';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-login',
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
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  form?: FormGroup;
  isLoading = false;
  showPassword = false;
  returnUrl: string;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private accountService: AccountService
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
  }

  ngOnInit() {
    this.form = this.fb.group({
      userName: this.fb.control('', [Validators.required]),
      password: this.fb.control('', [Validators.required, Validators.minLength(8)]),
      rememberLogin: this.fb.control(false)
    });
  }

  externalLogin(provider: string) {
    this.accountService.externalLogin(provider, this.returnUrl);
  }

  onSubmit() {
    if (this.isLoading || this.form?.invalid) return;
    this.isLoading = true;
    this.accountService.login(this.form?.value)
      .subscribe({
        next: () => {
          this.router.navigateByUrl(this.returnUrl);
        },
        error: () => {

        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }
}
