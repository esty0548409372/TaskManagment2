import { Injectable } from '@angular/core';
import { Project } from '../models/project';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { Worker } from '../models/worker';
import { sha256 } from 'js-sha256';
import { GlobalService } from './global.service';

@Injectable({
  providedIn: 'root'
})
export class ManagerService {
  workerToUpdate: Worker;
  idWorkerToDelete: any;
  isEdit: string = "Add";
  str:string;
  ids:number[];
  constructor(private http: HttpClient) {
    this.workerToUpdate = new Worker;
  }

  addProject(project): any {
    return this.http.post(GlobalService.path+"addProject/", JSON.parse(JSON.stringify(project)))
  }
  getAllManagers(): any {
    return this.http.get(GlobalService.path+"getAllManagers");
  }
  
  addWorker(worker): any {
    worker.Password=sha256(worker.Password);
      return this.http.post(GlobalService.path+"addWorker/", JSON.parse(JSON.stringify(worker)))
}
  getAllWorkers(): any {
    return this.http.get(GlobalService.path+"getAllWorkers");
  }
  deleteWorker() {
    return this.http.delete(GlobalService.path+"deleteWorker/" + this.idWorkerToDelete)
  }
  updateWorker(worker): any {
    worker.Id = this.workerToUpdate.Id;
    return this.http.put(GlobalService.path+"updateWorker/", JSON.parse(JSON.stringify(worker)))
  }
  addWorkersToProject(workers:Worker[],name):any{
    this.ids=[];
    workers.forEach(w => {
      this.ids.push(w.Id);
    });

    return this.http.post(GlobalService.path+"addWorkersToProject/"+name+"/", JSON.parse(JSON.stringify(this.ids)))

  }
 
  getPresence():any{
     return this.http.get(GlobalService.path+"getPresence");
  }
}
