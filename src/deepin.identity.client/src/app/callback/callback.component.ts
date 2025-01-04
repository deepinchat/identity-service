import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../core/services/account.service';

@Component({
  selector: 'app-callback',
  imports: [],
  templateUrl: './callback.component.html',
  styleUrl: './callback.component.scss'
})
export class CallbackComponent implements OnInit {
  type?: string;
  errorMessage?: string;
  constructor(private route: ActivatedRoute, private router: Router, private accountService: AccountService) {
    if (this.route.snapshot.paramMap.has('type')) {
      this.type = this.route.snapshot.queryParamMap.get('type') || '';
    }
  }

  ngOnInit() {
    if (this.type === 'external-login') {
      this.externalLoginCallback(this.route.snapshot.queryParams['returnUrl'], this.route.snapshot.queryParams['remoteError']);
    }
  }

  private externalLoginCallback(returnUrl: string, remoteError = '') {
    if (remoteError) {
      this.errorMessage = remoteError;
      return;
    }
    this.accountService.externalCallback()
      .subscribe({
        next: (x) => {
          console.log('got value ' + x);
          if (returnUrl) {
            location.href = returnUrl;
          } else {
            this.router.navigate(['/']);
          }
        },
        error(err) { console.error('something wrong occurred: ' + err); },
        complete() { console.log('done'); }
      });
  }
}
