using StreetWars.Game;
using StreetWars.Services;

namespace StreetWars;

public partial class MainPage : ContentPage
{
    private readonly IStreetWarsClient client;
    private RoomInfo? selectedRoom;
    private string? selectedCardId;
    private string currentPlayerId = "A";

    public MainPage()
    {
        InitializeComponent();
        client = Application.Current!.Handler.MauiContext!.Services.GetRequiredService<IStreetWarsClient>();
        client.StateChanged += OnStateChanged;
        _ = RefreshRoomsAsync();
    }

    private async void RefreshRoomsClicked(object sender, EventArgs e) =>
        await RefreshRoomsAsync();

    private async Task RefreshRoomsAsync()
    {
        try
        {
            RoomsView.ItemsSource = await client.GetRoomsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private void RoomSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        selectedRoom = e.CurrentSelection.FirstOrDefault() as RoomInfo;

    private async void CreateRoomClicked(object sender, EventArgs e)
    {
        try
        {
            var access = await client.CreateRoomAsync();
            await EnterRoomAsync(access);
            await RefreshRoomsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private async void JoinSelectedRoomClicked(object sender, EventArgs e)
    {
        try
        {
            if (selectedRoom is null)
                throw new InvalidOperationException("Selecione uma sala disponível.");

            var access = await client.JoinRoomAsync(selectedRoom.Id);
            await EnterRoomAsync(access);
            selectedRoom = null;
            RoomsView.SelectedItem = null;
            await RefreshRoomsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private async void CreateAiClicked(object sender, EventArgs e)
    {
        try
        {
            var access = await client.CreateAiGameAsync();
            await EnterRoomAsync(access);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private async Task EnterRoomAsync(RoomAccess access)
    {
        currentPlayerId = access.PlayerId;
        SessionEntry.Text = access.SessionId;
        PlayerEntry.Text = access.PlayerId;

        await client.ConnectAsync(access);
        Render(await client.GetStateAsync());
    }

    private async void PlayClicked(object sender, EventArgs e)
    {
        try
        {
            if (!int.TryParse(TerritoryEntry.Text, out var territory))
                throw new InvalidOperationException("Informe um território entre 0 e 4.");

            if (string.IsNullOrWhiteSpace(selectedCardId))
                throw new InvalidOperationException("Selecione uma carta da mão.");

            await client.PlayCarAsync(selectedCardId, territory);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private async void AttackClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(AttackerEntry.Text) || string.IsNullOrWhiteSpace(TargetEntry.Text))
                throw new InvalidOperationException("Informe o ID do atacante e do alvo.");

            await client.AttackAsync(AttackerEntry.Text.Trim(), TargetEntry.Text.Trim());
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private async void EndTurnClicked(object sender, EventArgs e)
    {
        try
        {
            await client.EndTurnAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("StreetWars", ex.Message, "OK");
        }
    }

    private void OnStateChanged(GameSnapshot snapshot) =>
        MainThread.BeginInvokeOnMainThread(() => Render(snapshot));

    private void Render(GameSnapshot snapshot)
    {
        StatusLabel.Text = snapshot.Winner is null
            ? $"Turno: Player {snapshot.ActivePlayer}"
            : $"Fim de jogo: Player {snapshot.Winner} venceu.";

        var me = snapshot.Players.FirstOrDefault(p =>
            p.Id.Equals(currentPlayerId, StringComparison.OrdinalIgnoreCase));

        ScoreLabel.Text = me is null
            ? $"Sala: {snapshot.Players.Count} jogadores"
            : $"Você: Player {me.Id} | Vida {me.Life} | Mana {me.Mana}";

        PlayersView.ItemsSource = snapshot.Players;
        TerritoriesView.ItemsSource = snapshot.Territories;

        HandLayout.Children.Clear();
        selectedCardId = null;

        foreach (var card in me?.Hand ?? [])
        {
            var button = new Button
            {
                Text = $"{card.Model}\nATK {card.Attack} / HP {card.Life}",
                CommandParameter = card.Id
            };

            button.Clicked += (_, _) =>
            {
                selectedCardId = (string)button.CommandParameter;
                foreach (var child in HandLayout.Children.OfType<Button>())
                    child.BackgroundColor = Colors.Transparent;
                button.BackgroundColor = Colors.LightGray;
            };

            HandLayout.Children.Add(button);
        }
    }
}