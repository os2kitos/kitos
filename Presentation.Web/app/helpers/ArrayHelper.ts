module Kitos.Helpers {
    
    export class ArrayHelper {

        /**
          *  Moves an element to the right hand side of another element in an array
          * @param arr - array containing elements
          * @param fromIndex - index of the element to be moved
          * @param toIndex - index of the element the first element is to be moved to
          */
        static arrayMoveElementToRightSide(arr: any[], fromIndex: number, toIndex: number){
            //we want to move the related column to "right" side of the base column
            toIndex += 1;

            this.arrayMoveElementTo(arr, fromIndex, toIndex);
        }

        /**
          *  Moves an element to the position of another element in an array
          * @param arr - array containing elements
          * @param fromIndex - index of the element to be moved
          * @param toIndex - index of the element the first element is to be moved to
          */
        static arrayMoveElementTo(arr: any[], fromIndex: number, toIndex: number){
            const element = arr[fromIndex];
            arr.splice(fromIndex, 1);
            arr.splice(toIndex, 0, element);
        }
    }
}