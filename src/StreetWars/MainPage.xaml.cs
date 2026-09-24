using StreetWars.Game;
using StreetWars.Services;

namespace StreetWars;

public partial class MainPage : ContentPage
{
    private readonly IStreetWarsClient client;
    private string? selectedCardId;

    public MainPage()
    {
        InitializeComponent();
        client = Application.Current!.Handler.MauiContext!.Services.GetRequiredService<IStreetWarsClient>();
        client.StateChanged += OnStateChanged;
        _ = client.ConnectAsync();
    }

    private async void CreateAiClicked(object sender, EventArgs e)
    {
        try
        {
            var snapshot = await client.CreateGameAsync(true);
            SessionEntry.Text = snapshot.SessionId;
            PlayerEntry.Text = "A";
            Render(snapshot);
        }
        catch (Exception ex)
        {
            await DisplayAlert("StreetWars", ex.Message, "OK");
        }
    }

    private async void JoinClicked(object sender, EventArgs e)
    {
        try
        {
            var snapshot = await client.JoinGameAsync(SessionEntry.Text.Trim());
            Render(snapshot);
        }
        catch (Exception ex)
        {
            await DisplayAlert("StreetWars", ex.Message, "OK");
        }
    }

    private async void PlayClicked(object sender, EventArgs e)
    {
        try
        {
            if (!int.TryParse(TerritoryEntry.Text, out var territory))
                throw new InvalidOperationException("Informe um território entre 0 e 4.");

            if (string.IsNullOrWhiteSpace(selectedCardId))
                throw new InvalidOperationException("Selecione uma carta da mão.");

            await client.PlayCarAsync(SessionEntry.Text.Trim(), PlayerEntry.Text.Trim(), selectedCardId, territory);
        }
        catch (Exception ex)
        {
            await DisplayAlert("StreetWars", ex.Message, "OK");
        }
    }

    private async void AttackClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(AttackerEntry.Text) || string.IsNullOrWhiteSpace(TargetEntry.Text))
                throw new InvalidOperationException("Informe o ID do atacante e do alvo.");

            await client.AttackAsync(
                SessionEntry.Text.Trim(),
                PlayerEntry.Text.Trim(),
                AttackerEntry.Text.Trim(),
                TargetEntry.Text.Trim());
        }
        catch (Exception ex)
        {
            await DisplayAlert("StreetWars", ex.Message, "OK");
        }
    }

    private async void EndTurnClicked(object sender, EventArgs e)
    {
        try
        {
            await client.EndTurnAsync(SessionEntry.Text.Trim(), PlayerEntry.Text.Trim());
        }
        catch (Exception ex)
        {
            await DisplayAlert("StreetWars", ex.Message, "OK");
        }
    }

    private void OnStateChanged(GameSnapshot snapshot) =>
        MainThread.BeginInvokeOnMainThread(() => Render(snapshot));

    private void Render(GameSnapshot snapshot)
    {
        StatusLabel.Text = snapshot.Winner is null
            ? $"Turno: Player {snapshot.ActivePlayer}"
            : $"Fim de jogo: Player {snapshot.Winner} venceu.";

        ScoreLabel.Text = $"A: {snapshot.PlayerALife} vida | B: {snapshot.PlayerBLife}";
        TerritoriesView.ItemsSource = snapshot.Territories;

        HandLayout.Children.Clear();
        var player = PlayerEntry.Text.Trim().Equals("B", StringComparison.OrdinalIgnoreCase)
            ? snapshot.PlayerBHand
            : snapshot.PlayerAHand;

        foreach (var card in player)
        {
            var button = new Button
            {
                Text = $"{card.Model}\nATK {card.Attack} / HP {card.Life}",
                CommandParameter = card.Id
            };

            button.Clicked += (_, _) =>
            {
                selectedCardId = (string)button.CommandParameter;
                AttackerEntry.Text = selectedCardId;
                foreach (var child in HandLayout.Children.OfType<Button>())
                    child.BackgroundColor = Colors.Transparent;
                button.BackgroundColor = Colors.LightGray;
            };

            HandLayout.Children.Add(button);
        }
    }
}