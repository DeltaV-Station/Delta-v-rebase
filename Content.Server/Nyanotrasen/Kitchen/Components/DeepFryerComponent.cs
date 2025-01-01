using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition;
using Content.Shared.Nyanotrasen.Kitchen;
using Content.Shared.Nyanotrasen.Kitchen.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Nyanotrasen.Kitchen.Components;

// TODO: move to shared and get rid of SharedDeepFryerComponent
[RegisterComponent, Access(typeof(SharedDeepfryerSystem))]
public sealed partial class DeepFryerComponent : SharedDeepFryerComponent
{
    // There are three levels to how the deep fryer treats entities.
    //
    // 1. An entity can be rejected by the blacklist and be untouched by
    //    anything other than heat damage.
    //
    // 2. An entity can be deep-fried but not turned into an edible. The
    //    change will be mostly cosmetic. Any entity that does not match
    //    the blacklist will fall into this category.
    //
    // 3. An entity can be deep-fried and turned into something edible. The
    //    change will permit the item to be permanently destroyed by eating
    //    it.

    /// <summary>
    /// When will the deep fryer layer on the next stage of crispiness?
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextFryTime;

    /// <summary>
    /// How much waste needs to be added at the next update interval?
    /// </summary>
    [DataField]
    public FixedPoint2 WasteToAdd = FixedPoint2.Zero;

    /// <summary>
    /// How often are items in the deep fryer fried?
    /// </summary>
    [DataField]
    public TimeSpan FryInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// What entities cannot be deep-fried no matter what?
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// What entities can be deep-fried into being edible?
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// What are over-cooked and burned entities turned into?
    /// </summary>
    /// <remarks>
    /// To prevent unwanted destruction of items, only food can be turned
    /// into this.
    /// </remarks>
    [DataField]
    public EntProtoId? CharredPrototype;

    /// <summary>
    /// What reagents are considered valid cooking oils?
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ReagentPrototype>> FryingOils = new();

    /// <summary>
    /// What reagents are added to tasty deep-fried food?
    /// </summary>
    [DataField]
    public List<ReagentQuantity> GoodReagents = new();

    /// <summary>
    /// What reagents are added to terrible deep-fried food?
    /// </summary>
    [DataField]
    public List<ReagentQuantity> BadReagents = new();

    /// <summary>
    /// What reagents replace every 1 unit of oil spent on frying?
    /// </summary>
    [DataField]
    public List<ReagentQuantity> WasteReagents = new();

    /// <summary>
    /// What flavors go well with deep frying?
    /// </summary>
    [DataField]
    public HashSet<ProtoId<FlavorPrototype>> GoodFlavors = new();

    /// <summary>
    /// What flavors don't go well with deep frying?
    /// </summary>
    [DataField]
    public HashSet<ProtoId<FlavorPrototype>> BadFlavors = new();

    /// <summary>
    /// How much is the price coefficiency of a food changed for each good flavor?
    /// </summary>
    [DataField]
    public float GoodFlavorPriceBonus = 0.2f;

    /// <summary>
    /// How much is the price coefficiency of a food changed for each bad flavor?
    /// </summary>
    [DataField]
    public float BadFlavorPriceMalus = -0.3f;

    /// <summary>
    /// What is the name of the solution container for the fryer's oil?
    /// </summary>
    [DataField]
    public string SolutionName  = "vat_oil";

    // TODO: Entity<SolutionComponent>
    public Solution Solution = default!;

    /// <summary>
    /// What is the name of the entity container for items inside the deep fryer?
    /// </summary>
    [DataField("storage")]
    public string StorageName = "vat_entities";

    public BaseContainer Storage = default!;

    /// <summary>
    /// How much solution should be imparted based on an item's size?
    /// </summary>
    [DataField]
    public FixedPoint2 SolutionSizeCoefficient = 1f;

    /// <summary>
    /// What's the maximum amount of solution that should ever be imparted?
    /// </summary>
    [DataField]
    public FixedPoint2 SolutionSplitMax = 10f;

    /// <summary>
    /// What percent of the fryer's solution has to be oil in order for it to fry?
    /// </summary>
    /// <remarks>
    /// The chef will have to clean it out occasionally, and if too much
    /// non-oil reagents are added, the vat will have to be drained.
    /// </remarks>
    [DataField]
    public FixedPoint2 FryingOilThreshold = 0.5f;

    /// <summary>
    /// What is the bare minimum number of oil units to prevent the fryer
    /// from unsafe operation?
    /// </summary>
    [DataField]
    public FixedPoint2 SafeOilVolume = 10f;

    [DataField]
    public List<EntityEffect> UnsafeOilVolumeEffects = new();

    /// <summary>
    /// What is the temperature of the vat when the deep fryer is powered?
    /// </summary>
    [DataField]
    public float PoweredTemperature = 550.0f;

    /// <summary>
    /// How many entities can this deep fryer hold?
    /// </summary>
    [DataField]
    public int StorageMaxEntities = 4;

    /// <summary>
    /// What sound is played when an item is inserted into hot oil?
    /// </summary>
    [DataField]
    public SoundSpecifier SoundInsertItem = new SoundPathSpecifier("/Audio/Nyanotrasen/Machines/deepfryer_basket_add_item.ogg");

    /// <summary>
    /// What sound is played when an item is removed?
    /// </summary>
    [DataField]
    public SoundSpecifier SoundRemoveItem = new SoundPathSpecifier("/Audio/Nyanotrasen/Machines/deepfryer_basket_remove_item.ogg");
}
