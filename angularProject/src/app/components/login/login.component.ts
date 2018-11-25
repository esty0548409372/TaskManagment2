import { Component } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { validate } from '../../shared/validate';
// import { Global, AuthenticationService, createValidatorArr } from '../../imports';
import sha256 from 'async-sha256';
import { GlobalService } from '../../shared/service/global.service';
import { DialogService } from '../../../../node_modules/ng2-bootstrap-modal';
import { ChangePasswordComponent } from '../change-password/change-password.component';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  //----------------PROPERTIRS-------------------

  loginFormGroup: FormGroup;
  isExistUser: boolean = true;
  //allow access 'Object' type via interpolation
  objectHolder: typeof Object = Object;

  //----------------CONSTRUCTOR------------------

  constructor(private formBuilder: FormBuilder, private router: Router, private globalService: GlobalService,private dialogService: DialogService) {
    this.loginFormGroup = this.formBuilder.group({
      userName: ['', validate.createValidatorArr("userName", 2, 10)],
      password: ['', validate.createValidatorArr("password", 6, 10)],
    });
    if(localStorage.getItem('currentUser')!=null)
    {
      this.router.navigate(['taskManagers/home']);
    }
  }

  //----------------METHODS-------------------

 async onSubmit() {
    const hash = await sha256(this.password.value);
    this.globalService.login(this.userName.value,hash)
      .subscribe(worker => { 
          localStorage.setItem('currentUser', JSON.stringify(worker));
          this.router.navigate(['taskManagers/home']);
        },err=>{
        this.isExistUser=false;
            this.router.navigate(['taskManagers/login']);
      })
  }


  //----------------GETTERS-------------------

  //getters of the form group controls

  get userName() {
    return this.loginFormGroup.controls["userName"];
  }
  get password() {
    return this.loginFormGroup.controls["password"];
  }

  changePassword(){
    let disposable = this.dialogService.addDialog(ChangePasswordComponent, {
      title: 'Change password',
    })
      .subscribe((isConfirmed) => {
        //We get dialog result
        if (isConfirmed) {
          
        }
        else {
          //alert('declined');
        }
      });
    setTimeout(() => {
      disposable.unsubscribe();
    }, 1000000);
  }
}