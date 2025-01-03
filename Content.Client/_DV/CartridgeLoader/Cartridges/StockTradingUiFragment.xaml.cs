using System.Linq;
using Content.Client.Administration.UI.CustomControls;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._DV.CartridgeLoader.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class StockTradingUiFragment : BoxContainer
{
    private readonly Dictionary<int, CompanyEntry> _companyEntries = new();

    // Event handlers for the parent UI
    public event Action<int, int>? OnBuyButtonPressed;
    public event Action<int, int>? OnSellButtonPressed;

    // Define colors
    public static readonly Color PositiveColor = Color.FromHex("#00ff00"); // Green
    public static readonly Color NegativeColor = Color.FromHex("#ff0000"); // Red
    public static readonly Color NeutralColor = Color.FromHex("#ffffff"); // White
    public static readonly Color BackgroundColor = Color.FromHex("#25252a"); // Dark grey
    public static readonly Color PriceBackgroundColor = Color.FromHex("#1a1a1a"); // Darker grey
    public static readonly Color BorderColor = Color.FromHex("#404040"); // Light grey

    public StockTradingUiFragment()
    {
        RobustXamlLoader.Load(this);
    }

    public void UpdateState(StockTradingUiState state)
    {
        NoEntries.Visible = state.Entries.Count == 0;
        Balance.Text = Loc.GetString("stock-trading-balance", ("balance", state.Balance));

        // Clear all existing entries
        foreach (var entry in _companyEntries.Values)
        {
            entry.Container.RemoveAllChildren();
        }
        _companyEntries.Clear();
        Entries.RemoveAllChildren();

        // Add new entries
        for (var i = 0; i < state.Entries.Count; i++)
        {
            var company = state.Entries[i];
            var entry = new CompanyEntry(i, company.LocalizedDisplayName, OnBuyButtonPressed, OnSellButtonPressed);
            _companyEntries[i] = entry;
            Entries.AddChild(entry.Container);

            var ownedStocks = state.OwnedStocks.GetValueOrDefault(i, 0);
            entry.Update(company, ownedStocks);
        }
    }

    private sealed class CompanyEntry
    {
        public readonly BoxContainer Container;
        private readonly Label _nameLabel;
        private readonly Label _priceLabel;
        private readonly Label _changeLabel;
        private readonly Button _sellButton;
        private readonly Button _buyButton;
        private readonly Label _sharesLabel;
        private readonly LineEdit _amountEdit;
        private readonly PriceHistoryTable _priceHistory;

        public CompanyEntry(int companyIndex,
            string displayName,
            Action<int, int>? onBuyPressed,
            Action<int, int>? onSellPressed)
        {
            Container = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                Margin = new Thickness(0, 0, 0, 2),
            };

            // Company info panel
            var companyPanel = new PanelContainer();

            var mainContent = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                Margin = new Thickness(8),
            };

            // Top row with company name and price info
            var topRow = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
            };

            _nameLabel = new Label
            {
                HorizontalExpand = true,
                Text = displayName,
            };

            // Create a panel for price and change
            var pricePanel = new PanelContainer
            {
                HorizontalAlignment = HAlignment.Right,
            };

            // Style the price panel
            var priceStyleBox = new StyleBoxFlat
            {
                BackgroundColor = BackgroundColor,
                ContentMarginLeftOverride = 8,
                ContentMarginRightOverride = 8,
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 4,
                BorderColor = BorderColor,
                BorderThickness = new Thickness(1),
            };

            pricePanel.PanelOverride = priceStyleBox;

            // Container for price and change labels
            var priceContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
            };

            _priceLabel = new Label();

            _changeLabel = new Label
            {
                HorizontalAlignment = HAlignment.Right,
                Modulate = NeutralColor,
                Margin = new Thickness(15, 0, 0, 0),
            };

            priceContainer.AddChild(_priceLabel);
            priceContainer.AddChild(_changeLabel);
            pricePanel.AddChild(priceContainer);

            topRow.AddChild(_nameLabel);
            topRow.AddChild(pricePanel);

            // Add the top row
            mainContent.AddChild(topRow);

            // Add the price history table between top and bottom rows
            _priceHistory = new PriceHistoryTable();
            mainContent.AddChild(_priceHistory);

            // Trading controls (bottom row)
            var bottomRow = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                Margin = new Thickness(0, 5, 0, 0),
            };

            _sharesLabel = new Label
            {
                Text = Loc.GetString("stock-trading-owned-shares"),
                MinWidth = 100,
            };

            _amountEdit = new LineEdit
            {
                PlaceHolder = Loc.GetString("stock-trading-amount-placeholder"),
                HorizontalExpand = true,
                MinWidth = 80,
            };

            var buttonContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalAlignment = HAlignment.Right,
                MinWidth = 140,
            };

            _buyButton = new Button
            {
                Text = Loc.GetString("stock-trading-buy-button"),
                MinWidth = 65,
                Margin = new Thickness(3, 0, 3, 0),
            };

            _sellButton = new Button
            {
                Text = Loc.GetString("stock-trading-sell-button"),
                MinWidth = 65,
            };

            buttonContainer.AddChild(_buyButton);
            buttonContainer.AddChild(_sellButton);

            bottomRow.AddChild(_sharesLabel);
            bottomRow.AddChild(_amountEdit);
            bottomRow.AddChild(buttonContainer);

            // Add the bottom row last
            mainContent.AddChild(bottomRow);

            companyPanel.AddChild(mainContent);
            Container.AddChild(companyPanel);

            // Add horizontal separator after the panel
            var separator = new HSeparator
            {
                Margin = new Thickness(5, 3, 5, 5),
            };
            Container.AddChild(separator);

            // Button click events
            _buyButton.OnPressed += _ =>
            {
                if (int.TryParse(_amountEdit.Text, out var amount) && amount > 0)
                    onBuyPressed?.Invoke(companyIndex, amount);
            };

            _sellButton.OnPressed += _ =>
            {
                if (int.TryParse(_amountEdit.Text, out var amount) && amount > 0)
                    onSellPressed?.Invoke(companyIndex, amount);
            };

            // There has to be a better way of doing this
            _amountEdit.OnTextChanged += args =>
            {
                var newText = string.Concat(args.Text.Where(char.IsDigit));
                if (newText != args.Text)
                    _amountEdit.Text = newText;
            };
        }

        public void Update(StockCompany company, int ownedStocks)
        {
            _nameLabel.Text = company.LocalizedDisplayName;
            _priceLabel.Text = $"${company.CurrentPrice:F2}";
            _sharesLabel.Text = Loc.GetString("stock-trading-owned-shares", ("shares", ownedStocks));

            var priceChange = 0f;
            if (company.PriceHistory is { Count: > 0 })
            {
                var previousPrice = company.PriceHistory[^1];
                priceChange = (company.CurrentPrice - previousPrice) / previousPrice * 100;
            }

            _changeLabel.Text = $"{(priceChange >= 0 ? "+" : "")}{priceChange:F2}%";

            // Update color based on price change
            _changeLabel.Modulate = priceChange switch
            {
                > 0 => PositiveColor,
                < 0 => NegativeColor,
                _ => NeutralColor,
            };

            // Update the price history table if not null
            if (company.PriceHistory != null)
                _priceHistory.Update(company.PriceHistory);

            // Disable sell button if no shares owned
            _sellButton.Disabled = ownedStocks <= 0;
        }
    }
}
