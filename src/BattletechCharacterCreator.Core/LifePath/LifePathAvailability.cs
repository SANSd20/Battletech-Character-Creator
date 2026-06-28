namespace BattletechCharacterCreator.Core.LifePath;

public static class LifePathAvailability
{
    public static IReadOnlyList<LifePathModule> FilterChildhoods(
        IEnumerable<LifePathModule> modules,
        bool isClanAffiliation) =>
        modules
            .Where(module => isClanAffiliation || module.Id != "trueborn-creche")
            .ToArray();

    public static IReadOnlyList<LifePathModule> FilterLateChildhoods(
        IEnumerable<LifePathModule> modules,
        bool isClanAffiliation) =>
        modules
            .Where(module => isClanAffiliation
                ? module.Id != "late-high-school"
                : module.Id is not (
                    "late-clan-apprenticeship" or
                    "late-freeborn-sibko" or
                    "late-trueborn-sibko"))
            .ToArray();
}
