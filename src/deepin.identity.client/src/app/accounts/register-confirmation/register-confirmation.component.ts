import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { NgIf } from '@angular/common';
import { MatButton } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-register-confirmation',
  imports: [
    NgIf,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButton,
    MatCardModule
  ],
  templateUrl: './register-confirmation.component.html',
  styleUrl: './register-confirmation.component.scss'
})
export class RegisterConfirmationComponent {
  form?: FormGroup;
  isLoading = false;
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
