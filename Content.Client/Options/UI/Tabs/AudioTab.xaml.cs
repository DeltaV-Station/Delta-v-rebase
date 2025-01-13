using Content.Client.Audio;
using Content.Shared.CCVar;
using Content.Shared._Impstation.CCVar;
using Robust.Client.Audio;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Client.Options.UI.Tabs;

[GenerateTypedNameReferences]
public sealed partial class AudioTab : Control
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IAudioManager _audio = default!;

    public AudioTab()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        var masterVolume = Control.AddOptionPercentSlider(
            CVars.AudioMasterVolume,
            SliderVolumeMaster,
            scale: ContentAudioSystem.MasterVolumeMultiplier);
        masterVolume.ImmediateValueChanged += OnMasterVolumeSliderChanged;

        Control.AddOptionPercentSlider(
            CVars.MidiVolume,
            SliderVolumeMidi,
            scale: ContentAudioSystem.MidiVolumeMultiplier);

        Control.AddOptionPercentSlider(
            CCVars.AmbientMusicVolume,
            SliderVolumeAmbientMusic,
            scale: ContentAudioSystem.AmbientMusicMultiplier);

        Control.AddOptionPercentSlider(
            CCVars.AmbienceVolume,
            SliderVolumeAmbience,
            scale: ContentAudioSystem.AmbienceMultiplier);

        Control.AddOptionPercentSlider(
            CCVars.LobbyMusicVolume,
            SliderVolumeLobby,
            scale: ContentAudioSystem.LobbyMultiplier);

        Control.AddOptionPercentSlider(
            CCVars.InterfaceVolume,
            SliderVolumeInterface,
            scale: ContentAudioSystem.InterfaceMultiplier);

        Control.AddOptionPercentSlider( //Impstation Random Announcer System port start
            ImpCCVars.AnnouncerVolume,
            SliderVolumeAnnouncer,
            scale: ContentAudioSystem.AnnouncerMultiplier); //Impstation Random Announcer System port end

        Control.AddOptionSlider(
            CCVars.MaxAmbientSources,
            SliderMaxAmbienceSounds,
            _cfg.GetCVar(CCVars.MinMaxAmbientSourcesConfigured),
            _cfg.GetCVar(CCVars.MaxMaxAmbientSourcesConfigured));

        Control.AddOptionCheckBox(CCVars.LobbyMusicEnabled, LobbyMusicCheckBox);
        Control.AddOptionCheckBox(CCVars.RestartSoundsEnabled, RestartSoundsCheckBox);
        Control.AddOptionCheckBox(CCVars.EventMusicEnabled, EventMusicCheckBox);
        Control.AddOptionCheckBox(ImpCCVars.AnnouncerDisableMultipleSounds, AnnouncerDisableMultipleSoundsCheckBox); //Impstation Random Announcer System
        Control.AddOptionCheckBox(CCVars.AdminSoundsEnabled, AdminSoundsCheckBox);

        Control.Initialize();
    }

    private void OnMasterVolumeSliderChanged(float value)
    {
        // TODO: I was thinking of giving OptionsTabControlRow a flag to "set CVar immediately", but I'm deferring that
        // until there's a proper system for enforcing people don't close the window with pending changes.
        _audio.SetMasterGain(value);
    }
}
