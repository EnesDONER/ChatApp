export class UserModel{
    id:string = "";
    name: string = "";
    status: Status = Status.Offline;
    avatar: string = "";
  }
  export enum Status{
    Online,
    Offline
  }