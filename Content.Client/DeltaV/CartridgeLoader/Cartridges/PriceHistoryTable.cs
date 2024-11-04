using System.Linq;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed class PriceHistoryTable : BoxContainer
{
    private readonly GridContainer _grid;
    private static readonly Color PositiveColor = Color.FromHex("#00ff00");
    private static readonly Color NegativeColor = Color.FromHex("#ff0000");
    private static readonly Color NeutralColor = Color.FromHex("#ffffff");

    public PriceHistoryTable()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        Margin = new Thickness(0, 5, 0, 0);

        // Create header
        var header = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };

        header.AddChild(new Label
        {
            Text = "Price History",
            HorizontalExpand = true,
            StyleClasses = { "LabelSubText" }
        });

        AddChild(header);

        // Create a panel container with styled background
        var panel = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 2, 0, 0)
        };

        // Create and apply the style
        var styleBox = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#1a1a1a"),
            ContentMarginLeftOverride = 6,
            ContentMarginRightOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
            BorderColor = Color.FromHex("#404040"),
            BorderThickness = new Thickness(1),
        };

        panel.PanelOverride = styleBox;

        // Create a centering container
        var centerContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
        };

        // Create grid for price history
        _grid = new GridContainer
        {
            Columns = 5, // Display 5 entries per row
        };

        centerContainer.AddChild(_grid);
        panel.AddChild(centerContainer);
        AddChild(panel);
    }

    public void Update(List<float> priceHistory)
    {
        _grid.RemoveAllChildren();

        // Take last 5 prices
        var lastTenPrices = priceHistory.TakeLast(5).ToList();

        for (var i = 0; i < lastTenPrices.Count; i++)
        {
            var price = lastTenPrices[i];
            var previousPrice = i > 0 ? lastTenPrices[i - 1] : price;
            var priceChange = ((price - previousPrice) / previousPrice) * 100;

            var entryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinWidth = 80,
                HorizontalAlignment = HAlignment.Center,
            };

            var priceLabel = new Label
            {
                Text = $"${price:F2}",
                HorizontalAlignment = HAlignment.Center,
            };

            var changeLabel = new Label
            {
                Text = $"{(priceChange >= 0 ? "+" : "")}{priceChange:F2}%",
                HorizontalAlignment = HAlignment.Center,
                StyleClasses = { "LabelSubText" },
                Modulate = priceChange switch
                {
                    > 0 => PositiveColor,
                    < 0 => NegativeColor,
                    _ => NeutralColor,
                }
            };

            entryContainer.AddChild(priceLabel);
            entryContainer.AddChild(changeLabel);
            _grid.AddChild(entryContainer);
        }
    }
}
