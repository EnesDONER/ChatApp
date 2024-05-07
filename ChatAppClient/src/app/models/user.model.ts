import { GroupModel } from "./group.model";

export class UserModel{
    id:string = "";
    name: string = "";
    status: Status = Status.Offline;
    avatar: string = "";
    groups: GroupModel[]= [];
  }
  export enum Status{
    Online,
    Offline
  }