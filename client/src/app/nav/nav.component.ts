import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
model: any={}
loggedIn: boolean;

  constructor(public accountService: AccountService) { }  //public so we can use it in the template

  ngOnInit(): void {
   
  }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      console.log(response);
     
    }, error => {
      console.log(error);
    })
    
  }

  logout() {
    this.accountService.logout();
    
  }

  
}
