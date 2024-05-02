import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { UserModel } from '../../models/user.model';
import { ChatModel } from '../../models/chat.model';
import * as signalR from '@microsoft/signalr';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  users : UserModel[] = [];
  chats :ChatModel[] = [];
  selectedUserId: string = "";
  selectedUser: UserModel = new UserModel();
  user = new UserModel();
  hub: signalR.HubConnection | undefined;
  message:string="";
  constructor(private http:HttpClient){
    this.user = JSON.parse(localStorage.getItem("accessToken")??"");
    this.getUsers();
  
    this.hub = new signalR.HubConnectionBuilder().withUrl("https://localhost:7098/chat-hub",
    {
      skipNegotiation:true,
      transport:signalR.HttpTransportType.WebSockets
    })
    .build();

    this.hub.start().then(() => {
      console.log("Connection is started");

      this.hub?.invoke("Connect",this.user.id);

      this.hub?.on("Users",(res:UserModel)=>{

        this.users.find(p=>p.id == res.id)!.status = res.status;
        
      })
    }).catch((error) => {
      console.error("Error starting connection:", error);
    });
    this.hub?.on("Messages",(res:any)=>{
      if(this.selectedUserId == res.userId){
        this.chats.push(res);
      }
    })
    

  

  }

  getUsers(){
    this.http.get<UserModel[]>("https://localhost:7098/api/Chats/GetUsers").subscribe(res=>{
      this.users =res.filter(p => p.id != this.user.id )
    })
  }

  changeUser(user: UserModel){
    this.selectedUserId = user.id;
    this.selectedUser = user;
    this.http.get(`https://localhost:7098/api/Chats/GetChats?userId=${this.user.id}&toUserId=${this.selectedUserId}`)
    .subscribe((res:any)=>{
      this.chats =res;
    });
    //this.chats = Chats.filter(p=> p.toUserId == user.id && p.userId == "0" || p.userId == user.id && p.toUserId == "0");
  }
  logout(){
    localStorage.clear();
    document.location.reload();
  }
  sendMessage(){
    const data = {
      "userId": this.user.id,
      "toUserId": this.selectedUserId,
      "message": this.message
    }
    this.http.post<ChatModel>("https://localhost:7098/api/Chats/SendMessage",data).subscribe((res)=>{
      this.chats.push(res);
      this.message = "";
    });
    
  }
}

