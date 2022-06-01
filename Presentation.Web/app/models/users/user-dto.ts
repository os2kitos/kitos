module Kitos.Models.Users {
    export interface IUserWithEmailDTO extends Models.Generic.NamedEntity.NamedEntityDTO {
        email: string
    }
}