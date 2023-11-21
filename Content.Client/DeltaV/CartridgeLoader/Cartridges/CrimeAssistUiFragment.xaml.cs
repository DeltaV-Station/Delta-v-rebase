using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Client.Nyanotrasen.UserInterface;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Robust.Client.AutoGenerated;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class CrimeAssistUiFragment : BoxContainer
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public event Action<bool>? OnSync;
    private CrimeAssistUiState.UiStates _currentState;

    public CrimeAssistUiFragment()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        VerticalExpand = true;

        UpdateUI(CrimeAssistUiState.UiStates.MainMenu);

        StartButton.OnPressed += _ => UpdateUI(CrimeAssistUiState.UiStates.IsItTerrorism);
        HomeButton.OnPressed += _ => UpdateUI(CrimeAssistUiState.UiStates.MainMenu);
        YesButton.OnPressed += _ => AdvanceState(_currentState, true);
        NoButton.OnPressed += _ => AdvanceState(_currentState, false);
    }

    public void AdvanceState(CrimeAssistUiState.UiStates currentState, bool yesPressed)
    {
        CrimeAssistUiState.UiStates newState = currentState switch
        {
            CrimeAssistUiState.UiStates.IsItTerrorism => yesPressed ? CrimeAssistUiState.UiStates.Result_Terrorism : CrimeAssistUiState.UiStates.WasSomeoneAttacked,

            //assault branch
            CrimeAssistUiState.UiStates.WasSomeoneAttacked => yesPressed ? CrimeAssistUiState.UiStates.WasItSophont : CrimeAssistUiState.UiStates.ForcedMindbreakerToxin,
            CrimeAssistUiState.UiStates.WasItSophont => yesPressed ? CrimeAssistUiState.UiStates.DidVictimDie : CrimeAssistUiState.UiStates.Result_AnimalCruelty,
            CrimeAssistUiState.UiStates.DidVictimDie => yesPressed ? CrimeAssistUiState.UiStates.IsVictimRemovedFromBody : CrimeAssistUiState.UiStates.Result_Assault,
            CrimeAssistUiState.UiStates.IsVictimRemovedFromBody => yesPressed ? CrimeAssistUiState.UiStates.Result_Decorporealisation : CrimeAssistUiState.UiStates.WasDeathIntentional,
            CrimeAssistUiState.UiStates.WasDeathIntentional => yesPressed ? CrimeAssistUiState.UiStates.Result_Murder : CrimeAssistUiState.UiStates.Result_Manslaughter,

            //mindbreaker branch
            CrimeAssistUiState.UiStates.ForcedMindbreakerToxin => yesPressed ? CrimeAssistUiState.UiStates.Result_Mindbreaking : CrimeAssistUiState.UiStates.HadIllegitimateItem,

            //theft branch
            CrimeAssistUiState.UiStates.HadIllegitimateItem => yesPressed ? CrimeAssistUiState.UiStates.WasItAPerson : CrimeAssistUiState.UiStates.WasSuspectInARestrictedLocation,
            CrimeAssistUiState.UiStates.WasItAPerson => yesPressed ? CrimeAssistUiState.UiStates.Result_Kidnapping : CrimeAssistUiState.UiStates.WasSuspectSelling,
            CrimeAssistUiState.UiStates.WasSuspectSelling => yesPressed ? CrimeAssistUiState.UiStates.Result_BlackMarketeering : CrimeAssistUiState.UiStates.WasSuspectSeenTaking,
            CrimeAssistUiState.UiStates.WasSuspectSeenTaking => yesPressed ? CrimeAssistUiState.UiStates.IsItemExtremelyDangerous : CrimeAssistUiState.UiStates.Result_Possession,
            CrimeAssistUiState.UiStates.IsItemExtremelyDangerous => yesPressed ? CrimeAssistUiState.UiStates.Result_GrandTheft : CrimeAssistUiState.UiStates.Result_Theft,

            //trespassing branch
            CrimeAssistUiState.UiStates.WasSuspectInARestrictedLocation => yesPressed ? CrimeAssistUiState.UiStates.WasEntranceLocked : CrimeAssistUiState.UiStates.DidSuspectBreakSomething,
            CrimeAssistUiState.UiStates.WasEntranceLocked => yesPressed ? CrimeAssistUiState.UiStates.Result_BreakingAndEntering : CrimeAssistUiState.UiStates.Result_Trespass,

            //vandalism branch
            CrimeAssistUiState.UiStates.DidSuspectBreakSomething => yesPressed ? CrimeAssistUiState.UiStates.WereThereManySuspects : CrimeAssistUiState.UiStates.WasCrimeSexualInNature,
            CrimeAssistUiState.UiStates.WereThereManySuspects => yesPressed ? CrimeAssistUiState.UiStates.Result_Rioting : CrimeAssistUiState.UiStates.WasDamageSmall,
            CrimeAssistUiState.UiStates.WasDamageSmall => yesPressed ? CrimeAssistUiState.UiStates.Result_Vandalism : CrimeAssistUiState.UiStates.WasDestroyedItemImportantToStation,
            CrimeAssistUiState.UiStates.WasDestroyedItemImportantToStation => yesPressed ? CrimeAssistUiState.UiStates.IsLargePartOfStationDestroyed : CrimeAssistUiState.UiStates.Result_Endangerment,
            CrimeAssistUiState.UiStates.IsLargePartOfStationDestroyed => yesPressed ? CrimeAssistUiState.UiStates.Result_GrandSabotage : CrimeAssistUiState.UiStates.Result_Sabotage,

            //sexual branch
            CrimeAssistUiState.UiStates.WasCrimeSexualInNature => yesPressed ? CrimeAssistUiState.UiStates.Result_SexualHarrassment : CrimeAssistUiState.UiStates.WasSuspectANuisance,

            //nuisance branch
            CrimeAssistUiState.UiStates.WasSuspectANuisance => yesPressed ? CrimeAssistUiState.UiStates.FalselyReportingToSecurity : CrimeAssistUiState.UiStates.Result_Innocent,
            CrimeAssistUiState.UiStates.FalselyReportingToSecurity => yesPressed ? CrimeAssistUiState.UiStates.Result_PerjuryOrFalseReport : CrimeAssistUiState.UiStates.HappenInCourt,
            CrimeAssistUiState.UiStates.HappenInCourt => yesPressed ? CrimeAssistUiState.UiStates.Result_ContemptOfCourt : CrimeAssistUiState.UiStates.DuringActiveInvestigation,
            CrimeAssistUiState.UiStates.DuringActiveInvestigation => yesPressed ? CrimeAssistUiState.UiStates.Result_ObstructionOfJustice : CrimeAssistUiState.UiStates.ToCommandStaff,
            CrimeAssistUiState.UiStates.ToCommandStaff => yesPressed ? CrimeAssistUiState.UiStates.Result_Sedition : CrimeAssistUiState.UiStates.WasItCommandItself,
            CrimeAssistUiState.UiStates.WasItCommandItself => yesPressed ? CrimeAssistUiState.UiStates.Result_AbuseOfPower : CrimeAssistUiState.UiStates.Result_Hooliganism,
            _ => CrimeAssistUiState.UiStates.MainMenu
        };

        UpdateUI(newState);
    }

    public void UpdateUI(CrimeAssistUiState.UiStates state)
    {
        _currentState = state;
        bool isResult = state.ToString().StartsWith("Result");

        StartButton.Visible = state == CrimeAssistUiState.UiStates.MainMenu;
        YesButton.Visible = state != CrimeAssistUiState.UiStates.MainMenu && !isResult;
        NoButton.Visible = state != CrimeAssistUiState.UiStates.MainMenu && !isResult;
        HomeButton.Visible = state != CrimeAssistUiState.UiStates.MainMenu;
        Explanation.Visible = state != CrimeAssistUiState.UiStates.MainMenu;

        Subtitle.Visible = isResult; // Crime severity is displayed here
        Punishment.Visible = isResult; // Crime punishment is displayed here

        if (!isResult)
        {
            string question = $"\n[font size=15]{GetQuestionLocString(state)}[/font]";

            if (question.ToLower().Contains("sophont"))
            {
                string sophontExplanation = Loc.GetString("crime-assist-sophont-explanation");
                question += $"\n[font size=8][color=#999999]{sophontExplanation}[/color][/font]";
            }

            Title.SetMarkup(question);
            Subtitle.SetMarkup(string.Empty);
            Explanation.SetMarkup(string.Empty);
            Punishment.SetMarkup(string.Empty);
        }
        else
        {
            Title.SetMarkup("\n[bold][font size=23][color=#a4885c]" + GetCrimeNameLocString(state) + "[/color][/font][/bold]");
            Subtitle.SetMarkup("\n[font size=19]" + GetCrimeSeverityLocString(state) + "[/font]");
            Explanation.SetMarkup("\n[title]" + GetCrimeExplanationLocString(state) + "[/title]\n");
            Punishment.SetMarkup("[bold][font size=15]" + GetCrimePunishmentLocString(state) + "[/font][/bold]");
        }
    }

    private string GetQuestionLocString(CrimeAssistUiState.UiStates state)
    {
        return Loc.GetString($"crime-assist-question-{state.ToString().ToLower()}");
    }

    private string GetCrimeExplanationLocString(CrimeAssistUiState.UiStates state)
    {
        return Loc.GetString($"crime-assist-crimedetail-{state.ToString().ToLower().Remove(0, 7)}");
    }

    private string GetCrimeNameLocString(CrimeAssistUiState.UiStates state)
    {
        return Loc.GetString($"crime-assist-crime-{state.ToString().ToLower().Remove(0, 7)}");
    }

    private string GetCrimePunishmentLocString(CrimeAssistUiState.UiStates state)
    {
        return Loc.GetString($"crime-assist-crimepunishment-{state.ToString().ToLower().Remove(0, 7)}");
    }

    private string GetCrimeSeverityLocString(CrimeAssistUiState.UiStates state)
    {
        return state switch
        {
            CrimeAssistUiState.UiStates.Result_Innocent => "[color=#39a300]" + Loc.GetString("crime-assist-crimetype-innocent") + "[/color]",
            CrimeAssistUiState.UiStates.Result_AnimalCruelty => "[color=#7b7b30]" + Loc.GetString("crime-assist-crimetype-misdemeanour") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Theft => "[color=#7b7b30]" + Loc.GetString("crime-assist-crimetype-misdemeanour") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Trespass => "[color=#7b7b30]" + Loc.GetString("crime-assist-crimetype-misdemeanour") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Vandalism => "[color=#7b7b30]" + Loc.GetString("crime-assist-crimetype-misdemeanour") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Hooliganism => "[color=#7b7b30]" + Loc.GetString("crime-assist-crimetype-misdemeanour") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Manslaughter => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony"),
            CrimeAssistUiState.UiStates.Result_GrandTheft => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_BlackMarketeering => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Sabotage => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Mindbreaking => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Assault => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_AbuseOfPower => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Possession => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Endangerment => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_BreakingAndEntering => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Rioting => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_ContemptOfCourt => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_PerjuryOrFalseReport => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_ObstructionOfJustice => "[color=#7b5430]" + Loc.GetString("crime-assist-crimetype-felony") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Murder => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Terrorism => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_GrandSabotage => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Decorporealisation => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Kidnapping => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_Sedition => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            CrimeAssistUiState.UiStates.Result_SexualHarrassment => "[color=#7b2e30]" + Loc.GetString("crime-assist-crimetype-capital") + "[/color]",
            _ => ""
        };
    }
}
