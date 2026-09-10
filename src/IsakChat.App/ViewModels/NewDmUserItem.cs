using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Prikazna stavka za listu "Novi cet" - korisnik + da li vec postoji privatni cet sa njim
// (racuna se poredjenjem sa listom konverzacija, ucitanom PARALELNO sa listom korisnika).
public class NewDmUserItem
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool AlreadyChatting { get; init; }

    public static NewDmUserItem From(UserDto user, bool alreadyChatting) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        AlreadyChatting = alreadyChatting
    };
}
