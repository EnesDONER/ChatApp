import { GroupChatModel } from './../../models/group-chat.model';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { UserModel ,Status} from '../../models/user.model';
import { ChatModel } from '../../models/chat.model';
import * as signalR from '@microsoft/signalr';
import { FormsModule } from '@angular/forms';
import { GroupModel } from '../../models/group.model';
import { SearchPipe } from '../../pipes/search.pipe';


@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule,FormsModule,SearchPipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
 
  public Status = Status;
  filter:string="";
  users : UserModel[] = [];
  chats :ChatModel[] = [];
  groupChats :GroupChatModel[] = [];
  selectedUserId: string = "";
  selectedGroupId: string = "";
  selectedUser: UserModel = new UserModel();
  selectedGroup: GroupModel = new GroupModel();
  public user = new UserModel();
  hub: signalR.HubConnection | undefined;
  message:string="";
  unreadMessages:ChatModel[] = [];
  unreadGroupMessages:GroupChatModel[] = [];
  groups:GroupModel[]=[];



  constructor(private http:HttpClient){
    this.user = JSON.parse(localStorage.getItem("accessToken")??"");
    this.getUsers();
    this.getGroup();
  
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
    this.hub?.on("GroupMessages",(res:any)=>{
      if(this.selectedGroupId == res.groupId){
        this.groupChats.push(res);

      }
      if(this.selectedGroupId != res.groupId){
        this.unreadGroupMessages.push(res);

      }
    })
    
    this.hub?.on("Messages",(res:any)=>{
      if(this.selectedUserId == res.userId){
        this.chats.push(res);
      }
      if(this.selectedUser != res.userId && this.user.id == res.toUserId){
        this.unreadMessages.push(res);
        console.log(this.unreadMessages)
      }
    })
  

  }
  changeUnreadMessage(){
    this.unreadMessages = this.unreadMessages.filter(unreadMessage=> 
      unreadMessage.userId != this.selectedUserId &&
      unreadMessage.toUserId == this.user.id
    );
  }
  changeUnreadGroupMessage(){
    this.unreadGroupMessages = this.unreadGroupMessages.filter(unreadMessage=> 
      unreadMessage.groupId != this.selectedGroupId
    );
  }
  getGroup(){
    this.http.get<GroupModel[]>("https://localhost:7098/api/Chats/GetGroupById?userId="+this.user.id).subscribe(res=>{
      this.groups =res;
      console.log(this.groups);
    })
  }


  getUsers(){
    this.http.get<UserModel[]>("https://localhost:7098/api/Chats/GetUsers").subscribe(res=>{
      this.users =res.filter(p => p.id != this.user.id );
    })
  }
  countUnreadMessagesByUserId(userId: string): number {
    return this.unreadMessages.filter(message => message.userId === userId).length;
  }
  countUnreadGroupMessagesByUserId(groupId: string): number {
    return this.unreadGroupMessages.filter(message => message.groupId === groupId).length;
  }
  changeUser(user: UserModel){
    this.selectedUserId = user.id;
    this.selectedUser = user;

    this.http.get(`https://localhost:7098/api/Chats/GetChats?userId=${this.user.id}&toUserId=${this.selectedUserId}`)
    .subscribe((res:any)=>{
      this.chats =res;
    });
  this.changeUnreadMessage();
    
  this.selectedGroupId = "";
    this.selectedGroup = new GroupModel();
  }

  changeGroup(group: GroupModel){
    this.selectedGroupId = group.id;
    this.selectedGroup = group;

    this.http.get(`https://localhost:7098/api/Chats/GetGroupChats?groupId=${this.selectedGroupId}`)
    .subscribe((res:any)=>{
      this.groupChats =res;
    });
    this.selectedUser = new UserModel();
    this.selectedUserId = "";
    this.changeUnreadGroupMessage();
    
  
  }
  logout(){
    localStorage.clear();
    document.location.reload();
  }
  sendMessage(){
    if(this.message==""){
      return;
    }
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
  sendMessagetoGroup(){
    if(this.message==""){
      return;
    }
    const data = {
      "userId": this.user.id,
      "groupId": this.selectedGroupId,
      "message": this.message
    }
    this.http.post<GroupChatModel>("https://localhost:7098/api/Chats/SendMessageToGroup",data).subscribe((res)=>{
      this.groupChats.push(res);
      this.message = "";
    });

  }
}

