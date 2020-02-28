module Kitos.Mappers
{
    export class AccessModifierMapper {
        static mapAccessModifier(accessModifier: number) {
            switch (accessModifier) {
            case 0:
                return "Lokal";
            case 1:
                return "Offentlig";
            default:
                return null;
            }
        }
    }
}