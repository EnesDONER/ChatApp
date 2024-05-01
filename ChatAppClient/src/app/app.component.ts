import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  users = Users;
  chats = Chats;
  selectedUserId: string = "1";
  selectedUser: UserModel = {
    id: "1",
    name: "Vincent Porter",
    status: "left 7 min ago",
    avatar: "avatar1.png"
  };


  changeUser(user: UserModel){
    this.selectedUserId = user.id;
    this.selectedUser = user;

    this.chats = Chats.filter(p=> p.toUserId == user.id && p.userId == "0" || p.userId == user.id && p.toUserId == "0");
  }

}

export class UserModel{
  id:string = "";
  name: string = "";
  status: string = "";
  avatar: string = "";
}

export const Users: UserModel[] = [
  {
    id: "1",
    name: "Vincent Porter",
    status: "left 7 min ago",
    avatar: "avatar1.png"
  },
  {
    id: "2",
    name: "Aiden Chavez",
    status: "online",
    avatar: "avatar3.png"
  },
  {
    id: "3",
    name: "Christian Kelly",
    status: "offline since oct 28",
    avatar: "avatar3.png"
  }
]

export class ChatModel{
  userId: string = "";
  toUserId: string = "";
  date: string  ="";
  message: string = "";
}

export const Chats: ChatModel[] = [
  {
    userId: "0",
    toUserId: "1",
    date: new Date().toString(),
    message: "Hi Aiden, how are you? How is the project coming along?"
  },
  {
    userId: "1",
    toUserId: "0",
    date: new Date().toString(),
    message: "Are we meeting today?"
  },
  {
    userId: "1",
    toUserId: "0",
    date: new Date().toString(),
    message: "Project has been already finished and I have results to show you."
  }
]