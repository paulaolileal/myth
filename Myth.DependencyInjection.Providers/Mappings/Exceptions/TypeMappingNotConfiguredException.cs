namespace Myth.Mappings.Exceptions;

public class TypeMappingNotConfiguredException : Exception {

    public TypeMappingNotConfiguredException() : base("Auto Mapper was not injected or Type Mapping was not configured!") {
    }
}