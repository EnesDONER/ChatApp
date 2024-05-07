import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'search',
  standalone: true
})
export class SearchPipe implements PipeTransform {
  transform(value: any[], filter: string): any[] {
    if (!value || !filter) {
      return value;
    }
    return value.filter(v => v.name.includes(filter));
  }
}