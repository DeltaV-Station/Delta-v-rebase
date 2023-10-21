using Content.Client.Station;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Map.Components;

namespace Content.Client.Administration.UI.Tabs.ObjectsTab;

[GenerateTypedNameReferences]
public sealed partial class ObjectsTab : Control
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    private readonly List<ObjectsTabEntry> _objects = new();
    private List<ObjectsTabSelection> _selections = new();

    public event Action<BaseButton.ButtonEventArgs>? OnEntryPressed;

    public ObjectsTab()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        ObjectTypeOptions.OnItemSelected += ev =>
        {
            ObjectTypeOptions.SelectId(ev.Id);
            RefreshObjectList(_selections[ev.Id]);
        };

        foreach (var type in Enum.GetValues(typeof(ObjectsTabSelection)))
        {
            _selections.Add((ObjectsTabSelection)type!);
            ObjectTypeOptions.AddItem(Enum.GetName((ObjectsTabSelection)type)!);
        }

        RefreshObjectList(_selections[ObjectTypeOptions.SelectedId]);
    }

    private void RefreshObjectList(ObjectsTabSelection selection)
    {
        var entities = new List<EntityUid>();
        switch (selection)
        {
            case ObjectsTabSelection.Stations:
                entities.AddRange(_entityManager.EntitySysManager.GetEntitySystem<StationSystem>().Stations);
                break;
            case ObjectsTabSelection.Grids:
            {
                var query = _entityManager.AllEntityQueryEnumerator<MapGridComponent>();
                while (query.MoveNext(out var uid, out _))
                {
                    entities.Add(uid);
                }

                break;
            }
            case ObjectsTabSelection.Maps:
            {
                var query = _entityManager.AllEntityQueryEnumerator<MapComponent>();
                while (query.MoveNext(out var uid, out _))
                {
                    entities.Add(uid);
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
        }

        foreach (var control in _objects)
        {
            ObjectList.RemoveChild(control);
        }

        _objects.Clear();

        foreach (var entity in entities)
        {
            // TODO the server eitehr needs to send the entity's name, or it needs to ensure the client knows about the entity.
            var name = _entityManager.GetComponentOrNull<MetaDataComponent>(entity)?.EntityName ?? "Unknown Entity"; // this should be fixed, so I CBF localizing.
            var ctrl = new ObjectsTabEntry(name, entity);
            _objects.Add(ctrl);
            ObjectList.AddChild(ctrl);
            ctrl.OnPressed += args => OnEntryPressed?.Invoke(args);
        }
    }

    private enum ObjectsTabSelection
    {
        Grids,
        Maps,
        Stations,
    }
}

