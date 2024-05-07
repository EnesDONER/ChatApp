import { UserModel } from "./user.model";

export class GroupModel{
    id:string="";
    name:string="";
    avatar:string="";
    users:UserModel[]=[];
}