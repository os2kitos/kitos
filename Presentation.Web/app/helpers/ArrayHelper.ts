module Kitos.Helpers {
    
    export class ArrayHelper {

        /**
          *  Moves an element to the right hand side of another element in an array
          * @param arr - array containing elements
          * @param fromIndex - index of the element to be moved
          * @param rightSideOfIndex - index of the element the first element is to be moved to
          */
        static arrayMoveElementToRightSide(arr: any[], fromIndex: number, rightSideOfIndex: number){
            //we want to move the related column to "right" side of the base column
            const toIndex = rightSideOfIndex + 1;

            this.arrayMoveElementTo(arr, fromIndex, toIndex);
        }

        /**
          *  Moves an element to the position of another element in an array
          * @param arr - array containing elements
          * @param fromIndex - index of the element to be moved
          * @param toIndex - index of the element the first element is to be moved to
          */
        static arrayMoveElementTo(arr: any[], fromIndex: number, toIndex: number) {
            if (fromIndex < 0 || toIndex < 0)
                throw new RangeError("Index cannot be smaller than 0");
            if (fromIndex > arr.length)
                throw new RangeError(`FromIndex doesn't exist in the array, array length equals: ${arr.length}, while selected index equals: ${fromIndex}`)
            if (toIndex > arr.length)
                throw new RangeError(`ToIndex doesn't exist in the array, array length equals: ${arr.length}, while selected index equals: ${fromIndex}`)

            const element = arr[fromIndex];
            arr.splice(fromIndex, 1);
            arr.splice(toIndex, 0, element);
        }

        static concatFirstNumberOfItemsAndAddElipsis(array: Array<string>, numberOfItemsToConcat: number): string {
            let concatItems = "";

            if (array === undefined)
                return concatItems;

            // join the first x username together
            if (array.length > 0) {
                concatItems = array.slice(0, numberOfItemsToConcat).join(", ");
            }

            // if more than x then add an elipsis
            if (array.length > numberOfItemsToConcat) {
                concatItems += ", ...";
            }

            return concatItems;
        }
    }
}